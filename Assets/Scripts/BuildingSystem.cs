using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [Header("Estructuras")]
    public GameObject rampPrefab;
    public GameObject platformPrefab;
    public GameObject wallPrefab;

    [Header("Grid")]
    public float gridSize = 2f;
    public float buildDistance = 4f;

    [Header("Material Costo")]
    public int costPerStructure = 5;

    private bool buildMode = false;
    private int selectedIndex = -1;
    private GameObject previewObj;
    private GameObject[] prefabs;
    private float currentRotation = 0f;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        prefabs = new GameObject[] { rampPrefab, platformPrefab, wallPrefab };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            ToggleBuildMode();

        if (!buildMode) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectStructure(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectStructure(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectStructure(2);

        if (Input.GetKeyDown(KeyCode.Q)) currentRotation -= 90f;
        if (Input.GetKeyDown(KeyCode.E)) currentRotation += 90f;

        UpdatePreview();

        if (Input.GetMouseButtonDown(0) && previewObj != null)
            PlaceStructure();
    }

    void ToggleBuildMode()
    {
        buildMode = !buildMode;
        if (!buildMode)
        {
            if (previewObj != null) Destroy(previewObj);
            previewObj = null;
            selectedIndex = -1;
        }
        UIManager.Instance.UpdateBuildUI(buildMode, selectedIndex);
    }

    void SelectStructure(int index)
    {
        selectedIndex = index;
        if (previewObj != null) Destroy(previewObj);

        previewObj = Instantiate(prefabs[index]);
        SetPreviewMaterial(previewObj);
        UIManager.Instance.UpdateBuildUI(buildMode, selectedIndex);
    }

    void SetPreviewMaterial(GameObject obj)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            Material m = new Material(r.material);
            Color c = m.color;
            c.a = 0.4f;
            m.color = c;
            m.SetFloat("_Surface", 1);
            m.renderQueue = 3000;
            r.material = m;
        }

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void UpdatePreview()
    {
        if (previewObj == null) return;

        Vector3 spawnPos = transform.position + cam.transform.forward * 4f;
        spawnPos.y = transform.position.y;

        Vector3 snapped = SnapToGrid(spawnPos);
        previewObj.transform.position = snapped;
        previewObj.transform.rotation = GetStructureRotation();
    }

    Quaternion GetStructureRotation()
    {
        if (selectedIndex == 0) // Rampa
        {
            Quaternion baseRot = prefabs[0].transform.rotation;
            Quaternion yRot = Quaternion.Euler(0f, currentRotation, 0f);
            return yRot * baseRot;
        }
        return Quaternion.Euler(0f, currentRotation, 0f);
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        float z = Mathf.Round(pos.z / gridSize) * gridSize;
        return new Vector3(x, y, z);
    }

    void PlaceStructure()
    {
        if (!GameManager.Instance.SpendMaterials(costPerStructure))
        {
            Debug.Log("Sin materiales suficientes");
            return;
        }

        Instantiate(prefabs[selectedIndex],
            previewObj.transform.position,
            GetStructureRotation());

        GameManager.Instance.AddStructure();
        UIManager.Instance.UpdateHUD();
        UIManager.Instance.UpdateBuildUI(buildMode, selectedIndex);
    }
}