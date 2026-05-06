using UnityEngine;

/// <summary>
/// Sistema de construccion estilo Fortnite:
/// - Presionar B para activar/desactivar modo construccion.
/// - Teclas 1, 2, 3 para elegir la estructura (plataforma, pared, rampa).
/// - Se muestra un fantasma transparente donde quedaria.
/// - Click izquierdo para colocar (gasta tinta).
/// - Q / E para rotar la estructura.
/// </summary>
public class BuildingSystem : MonoBehaviour
{
    // -------------------------------------------------
    // Clase interna que describe cada tipo de estructura
    [System.Serializable]
    public class Structure
    {
        public string     name;     // Ej: "Plataforma"
        public GameObject prefab;   // Prefab real que se instancia
        public float      inkCost;  // Tinta que cuesta construirla
    }

    [Header("Estructuras disponibles (llenar en inspector)")]
    public Structure[] structures;   // Agregar 3 estructuras: piso, pared, rampa

    [Header("Materiales")]
    [Tooltip("Material transparente azulado para el fantasma")]
    public Material ghostMaterial;

    [Header("Configuracion")]
    public float buildRange  = 8f;  // Distancia maxima para colocar
    public float gridSize    = 2f;  // Tamano de la grilla de snap

    [Tooltip("Capas sobre las que se puede construir (ej: Default, Ground)")]
    public LayerMask buildLayerMask;

    // --- Estado interno ---
    private int        _selectedIndex = 0;
    private GameObject _ghostObject;
    private bool       _buildMode     = false;
    private float      _ghostRotY     = 0f;   // Rotacion en Y del fantasma
    private Camera     _cam;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGamePaused || !GameManager.Instance.IsLevelActive) return;

        HandleModeToggle();

        if (_buildMode)
        {
            HandleStructureSelection();
            HandleRotation();
            UpdateGhostPreview();
            HandlePlacement();
        }
    }

    // -----------------------------------------------------------------------
    // ACTIVAR / DESACTIVAR MODO CONSTRUCCION

    private void HandleModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            _buildMode = !_buildMode;
            UIManager.Instance.ShowBuildUI(_buildMode);

            if (_buildMode)
                SpawnGhost();
            else
                DestroyGhost();
        }
    }

    // -----------------------------------------------------------------------
    // SELECCIONAR ESTRUCTURA (teclas 1-2-3...)

    private void HandleStructureSelection()
    {
        for (int i = 0; i < structures.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                _selectedIndex = i;
                _ghostRotY     = 0f;
                SpawnGhost();
            }
        }
    }

    // -----------------------------------------------------------------------
    // ROTAR LA ESTRUCTURA CON Q / E

    private void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.Q)) _ghostRotY -= 90f;
        if (Input.GetKeyDown(KeyCode.E)) _ghostRotY += 90f;

        if (_ghostObject != null)
            _ghostObject.transform.rotation = Quaternion.Euler(0f, _ghostRotY, 0f);
    }

    // -----------------------------------------------------------------------
    // ACTUALIZAR POSICION DEL FANTASMA

    private void UpdateGhostPreview()
    {
        if (_ghostObject == null) return;

        // Raycast desde el centro de la pantalla hacia adelante
        Ray ray = _cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out RaycastHit hit, buildRange, buildLayerMask))
        {
            // Snap a la grilla
            Vector3 snapped = new Vector3(
                Mathf.Round(hit.point.x / gridSize) * gridSize,
                Mathf.Round(hit.point.y / gridSize) * gridSize,
                Mathf.Round(hit.point.z / gridSize) * gridSize
            );

            _ghostObject.transform.position = snapped;
            _ghostObject.SetActive(true);
        }
        else
        {
            // Si no hay superficie valida, ocultar el fantasma
            _ghostObject.SetActive(false);
        }
    }

    // -----------------------------------------------------------------------
    // COLOCAR LA ESTRUCTURA

    private void HandlePlacement()
    {
        // Click izquierdo para colocar
        if (Input.GetMouseButtonDown(0) && _ghostObject != null && _ghostObject.activeSelf)
        {
            Structure s = structures[_selectedIndex];

            if (PlayerStats.Instance.TrySpendInk(s.inkCost))
            {
                // Instanciar la estructura real en la posicion del fantasma
                Instantiate(s.prefab, _ghostObject.transform.position, _ghostObject.transform.rotation);
                SoundManager.Instance.PlaySound("Build");
            }
            else
            {
                // No habia suficiente tinta
                UIManager.Instance.ShowNotEnoughInk();
            }
        }
    }

    // -----------------------------------------------------------------------
    // CREAR / DESTRUIR FANTASMA

    private void SpawnGhost()
    {
        DestroyGhost();
        if (structures == null || structures.Length == 0) return;

        // Instanciar el prefab como fantasma
        _ghostObject = Instantiate(structures[_selectedIndex].prefab);
        _ghostObject.name = "Ghost_" + structures[_selectedIndex].name;

        // Reemplazar todos los materiales por el material transparente
        foreach (Renderer rend in _ghostObject.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = new Material[rend.materials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = ghostMaterial;
            rend.materials = mats;
        }

        // Eliminar colisiones del fantasma (solo visual)
        foreach (Collider col in _ghostObject.GetComponentsInChildren<Collider>())
            Destroy(col);

        // Eliminar cualquier script en el fantasma
        foreach (MonoBehaviour mb in _ghostObject.GetComponentsInChildren<MonoBehaviour>())
            Destroy(mb);

        _ghostObject.SetActive(false);
    }

    private void DestroyGhost()
    {
        if (_ghostObject != null)
        {
            Destroy(_ghostObject);
            _ghostObject = null;
        }
    }

    private void OnDestroy()
    {
        DestroyGhost();
    }
}
