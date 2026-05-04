#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor Tool: Menu → ThePainter → Build Scene
/// Ejecutar UNA VEZ en escena vacía.
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [MenuItem("ThePainter/Build Scene")]
    public static void BuildScene()
    {
        Debug.Log("[SceneSetup] Construyendo escena The Painter...");

        // ── Prefabs ──────────────────────────────────────────────────────
        GameObject playerPrefab = CreatePlayerPrefab();
        GameObject solidPrefab = CreatePlatformPrefab("Platform_Solid",
            new Vector3(2f, 0.3f, 2f), PaintedPlatform.SolidColor);
        GameObject elasticPrefab = CreatePlatformPrefab("Platform_Elastic",
            new Vector3(2f, 0.3f, 2f), PaintedPlatform.ElasticColor);
        GameObject explosivePrefab = CreatePlatformPrefab("Platform_Explosive",
            new Vector3(2f, 0.3f, 2f), PaintedPlatform.ExplosiveColor);
        GameObject indicatorPrefab = CreateIndicatorPrefab();
        GameObject dropletPrefab = CreateDropletPrefab();

        // Add PaintedPlatform script to platform prefabs
        solidPrefab.AddComponent<PaintedPlatform>();
        elasticPrefab.AddComponent<PaintedPlatform>();
        explosivePrefab.AddComponent<PaintedPlatform>();
        dropletPrefab.AddComponent<InkDroplet>();

        // ── Singletons ───────────────────────────────────────────────────
        GameObject gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        gmGO.AddComponent<SoundManager>();
        var sfxSource = gmGO.AddComponent<AudioSource>(); sfxSource.playOnAwake = false;
        var musicSource = gmGO.AddComponent<AudioSource>(); musicSource.playOnAwake = false;
        var smSO = new SerializedObject(gmGO.GetComponent<SoundManager>());
        smSO.FindProperty("audioSourceSFX").objectReferenceValue = sfxSource;
        smSO.FindProperty("audioSourceMusic").objectReferenceValue = musicSource;
        smSO.ApplyModifiedProperties();

        GameObject poolGO = new GameObject("ObjectPoolManager");
        poolGO.AddComponent<ObjectPool>();

        GameObject inkGO = new GameObject("InkSystem");
        inkGO.AddComponent<InkSystem>();

        GameObject paintMgrGO = new GameObject("PaintManager");
        paintMgrGO.AddComponent<PaintManager>();

        GameObject levelMgrGO = new GameObject("LevelManager");
        var lm = levelMgrGO.AddComponent<LevelManager>();

        // ── Level geometry ───────────────────────────────────────────────
        GameObject levelRoot = new GameObject("Level");

        // Floor
        GameObject floor = CreateBox("Floor", new Vector3(20f, 0.5f, 20f),
            new Color(0.15f, 0.15f, 0.15f), new Vector3(0f, -0.25f, 0f));
        floor.transform.SetParent(levelRoot.transform);
        SetLayer(floor, "Ground");

        // Walls
        GameObject wallN = CreateBox("Wall_N", new Vector3(20f, 6f, 0.5f),
            new Color(0.2f, 0.2f, 0.25f), new Vector3(0f, 3f, 10f));
        wallN.transform.SetParent(levelRoot.transform);

        GameObject wallS = CreateBox("Wall_S", new Vector3(20f, 6f, 0.5f),
            new Color(0.2f, 0.2f, 0.25f), new Vector3(0f, 3f, -10f));
        wallS.transform.SetParent(levelRoot.transform);

        GameObject wallE = CreateBox("Wall_E", new Vector3(0.5f, 6f, 20f),
            new Color(0.2f, 0.2f, 0.25f), new Vector3(10f, 3f, 0f));
        wallE.transform.SetParent(levelRoot.transform);

        GameObject wallW = CreateBox("Wall_W", new Vector3(0.5f, 6f, 20f),
            new Color(0.2f, 0.2f, 0.25f), new Vector3(-10f, 3f, 0f));
        wallW.transform.SetParent(levelRoot.transform);

        // Exit door
        GameObject door = CreateBox("ExitDoor", new Vector3(1.5f, 2.5f, 0.3f),
            new Color(0.3f, 0.3f, 0.3f), new Vector3(8f, 1.25f, 9.8f));
        door.transform.SetParent(levelRoot.transform);
        var doorTrigger = door.AddComponent<BoxCollider>();
        doorTrigger.isTrigger = true;
        doorTrigger.size = new Vector3(1f, 1f, 1f);
        door.AddComponent<ExitDoorTrigger>();
        // Wire door to LevelManager
        var lmSO = new SerializedObject(lm);
        lmSO.FindProperty("exitDoor").objectReferenceValue = door;
        lmSO.ApplyModifiedProperties();

        // Ink Droplets placed around the level
        Vector3[] dropletPositions =
        {
            new Vector3(-3f, 0.5f, 2f),
            new Vector3(3f, 0.5f, -2f),
            new Vector3(0f, 0.5f, 4f),
            new Vector3(-5f, 0.5f, -3f),
            new Vector3(5f, 0.5f, 3f),
        };
        foreach (var pos in dropletPositions)
        {
            GameObject d = GameObject.Instantiate(dropletPrefab, levelRoot.transform);
            d.transform.position = pos;
            d.name = "InkDroplet";
        }

        // Erasers
        GameObject eraserRoot = new GameObject("Erasers");
        eraserRoot.transform.SetParent(levelRoot.transform);

        GameObject walker = CreateBox("EraserWalker", new Vector3(0.8f, 0.8f, 0.8f),
            Color.black, new Vector3(-2f, 0.4f, 0f));
        walker.transform.SetParent(eraserRoot.transform);
        walker.tag = "Eraser";
        walker.AddComponent<EraserWalker>();

        GameObject flyer = CreateBox("EraserFlyer", new Vector3(0.7f, 0.7f, 0.7f),
            new Color(0.15f, 0.15f, 0.15f), new Vector3(2f, 2f, 3f));
        flyer.transform.SetParent(eraserRoot.transform);
        flyer.tag = "Eraser";
        flyer.AddComponent<EraserFlyer>();

        GameObject fat = CreateBox("EraserFat", new Vector3(1.5f, 1.5f, 1.5f),
            new Color(0.1f, 0.1f, 0.1f), new Vector3(5f, 0.75f, 5f));
        fat.transform.SetParent(eraserRoot.transform);
        fat.tag = "Eraser";
        fat.AddComponent<EraserFat>();

        // ── Player ───────────────────────────────────────────────────────
        GameObject playerGO = GameObject.Instantiate(playerPrefab);
        playerGO.name = "Player";
        playerGO.transform.position = new Vector3(-7f, 0.6f, -7f);
        playerGO.tag = "Player";

        var pc = playerGO.AddComponent<PlayerController>();
        var pb = playerGO.AddComponent<PaintBrush>();

        // Wire prefabs to PaintBrush
        var pbSO = new SerializedObject(pb);
        pbSO.FindProperty("solidPlatformPrefab").objectReferenceValue = solidPrefab;
        pbSO.FindProperty("elasticPlatformPrefab").objectReferenceValue = elasticPrefab;
        pbSO.FindProperty("explosivePlatformPrefab").objectReferenceValue = explosivePrefab;
        pbSO.FindProperty("paintIndicatorPrefab").objectReferenceValue = indicatorPrefab;
        pbSO.ApplyModifiedProperties();

        // CharacterController
        var cc = playerGO.AddComponent<CharacterController>();
        cc.height = 1.2f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.6f, 0f);

        // ── Camera ───────────────────────────────────────────────────────
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCam = camGO.AddComponent<Camera>();
        }
        var camCtrl = mainCam.GetComponent<CameraController>()
            ?? mainCam.gameObject.AddComponent<CameraController>();
        var camSO = new SerializedObject(camCtrl);
        camSO.FindProperty("target").objectReferenceValue = playerGO.transform;
        camSO.ApplyModifiedProperties();

        // ── Canvas HUD ───────────────────────────────────────────────────
        GameObject hudCanvas = CreateHUDCanvas();

        // ── Canvas UI ────────────────────────────────────────────────────
        GameObject uiCanvas = CreateUICanvas();

        // ── Wire GameManager ─────────────────────────────────────────────
        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("playerController").objectReferenceValue = pc;
        gmSO.FindProperty("levelManager").objectReferenceValue = lm;
        gmSO.FindProperty("uiManager").objectReferenceValue = uiCanvas.GetComponent<UIManager>();
        gmSO.FindProperty("hudManager").objectReferenceValue = hudCanvas.GetComponent<HUDManager>();
        gmSO.FindProperty("soundManager").objectReferenceValue = gmGO.GetComponent<SoundManager>();
        gmSO.ApplyModifiedProperties();

        // Deactivate prefab templates
        playerPrefab.SetActive(false);
        solidPrefab.SetActive(false);
        elasticPrefab.SetActive(false);
        explosivePrefab.SetActive(false);
        indicatorPrefab.SetActive(false);
        dropletPrefab.SetActive(false);

        Debug.Log("[SceneSetup] ¡Escena The Painter construida exitosamente!");
    }

    // ── Geometry Helpers ─────────────────────────────────────────────────────

    static GameObject CreateBox(string name, Vector3 scale, Color color, Vector3 pos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localScale = scale;
        go.transform.position = pos;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(color);
        return go;
    }

    static Material MakeMat(Color c)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = c;
        return m;
    }

    static void SetLayer(GameObject go, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer >= 0) go.layer = layer;
    }

    static GameObject CreatePlayerPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Player_Prefab";
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(Color.white);
        DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static GameObject CreatePlatformPrefab(string name, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(color);
        // Keep BoxCollider but NOT trigger (platforms are solid)
        return go;
    }

    static GameObject CreateIndicatorPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "PaintIndicator_Prefab";
        go.transform.localScale = new Vector3(2f, 0.05f, 2f);
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0.2f, 1f, 0.4f, 0.5f));
        DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static GameObject CreateDropletPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "InkDroplet_Prefab";
        go.transform.localScale = Vector3.one * 0.4f;
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(new Color(0f, 0.8f, 1f));
        var col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;
        return go;
    }

    // ── Canvas Builders ───────────────────────────────────────────────────────

    static GameObject CreateHUDCanvas()
    {
        var canvasGO = new GameObject("Canvas_HUD");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var hudRoot = new GameObject("HUDRoot");
        hudRoot.transform.SetParent(canvasGO.transform, false);

        // Ink bar
        var inkBarBG = CreateUIImage(hudRoot, "InkBarBG",
            new Color(0.1f, 0.1f, 0.1f, 0.8f),
            new Vector2(0f, 1f), new Vector2(220f, 24f), new Vector2(120f, -30f));

        var inkSliderGO = new GameObject("InkSlider");
        inkSliderGO.transform.SetParent(hudRoot.transform, false);
        var inkSlider = inkSliderGO.AddComponent<Slider>();
        var inkSliderRT = inkSliderGO.GetComponent<RectTransform>();
        inkSliderRT.anchorMin = new Vector2(0f, 1f);
        inkSliderRT.anchorMax = new Vector2(0f, 1f);
        inkSliderRT.sizeDelta = new Vector2(200f, 20f);
        inkSliderRT.anchoredPosition = new Vector2(120f, -30f);
        inkSlider.value = 1f;

        var inkFillArea = new GameObject("Fill Area");
        inkFillArea.transform.SetParent(inkSliderGO.transform, false);
        var inkFillAreaRT = inkFillArea.AddComponent<RectTransform>();
        inkFillAreaRT.anchorMin = Vector2.zero;
        inkFillAreaRT.anchorMax = Vector2.one;
        inkFillAreaRT.offsetMin = Vector2.zero;
        inkFillAreaRT.offsetMax = Vector2.zero;

        var inkFillGO = new GameObject("Fill");
        inkFillGO.transform.SetParent(inkFillArea.transform, false);
        var inkFillImg = inkFillGO.AddComponent<Image>();
        inkFillImg.color = new Color(0.2f, 0.8f, 1f);
        var inkFillRT = inkFillGO.GetComponent<RectTransform>();
        inkFillRT.anchorMin = Vector2.zero;
        inkFillRT.anchorMax = Vector2.one;
        inkFillRT.offsetMin = Vector2.zero;
        inkFillRT.offsetMax = Vector2.zero;
        inkSlider.fillRect = inkFillRT;

        // Ink label
        CreateTMPText(hudRoot, "InkLabel", "TINTA",
            new Vector2(0f, 1f), new Vector2(80f, 24f), new Vector2(30f, -30f), 13);

        // Paint percent text
        var pctText = CreateTMPText(hudRoot, "PaintPercentText", "0%",
            new Vector2(0.5f, 1f), new Vector2(120f, 36f), new Vector2(0f, -20f), 24);
        pctText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Paint percent slider
        var paintSliderGO = new GameObject("PaintSlider");
        paintSliderGO.transform.SetParent(hudRoot.transform, false);
        var paintSlider = paintSliderGO.AddComponent<Slider>();
        var paintSliderRT = paintSliderGO.GetComponent<RectTransform>();
        paintSliderRT.anchorMin = new Vector2(0.5f, 1f);
        paintSliderRT.anchorMax = new Vector2(0.5f, 1f);
        paintSliderRT.sizeDelta = new Vector2(200f, 14f);
        paintSliderRT.anchoredPosition = new Vector2(0f, -55f);

        var paintFillArea = new GameObject("Fill Area");
        paintFillArea.transform.SetParent(paintSliderGO.transform, false);
        var paintFillAreaRT = paintFillArea.AddComponent<RectTransform>();
        paintFillAreaRT.anchorMin = Vector2.zero;
        paintFillAreaRT.anchorMax = Vector2.one;
        paintFillAreaRT.offsetMin = Vector2.zero;
        paintFillAreaRT.offsetMax = Vector2.zero;

        var paintFillGO = new GameObject("Fill");
        paintFillGO.transform.SetParent(paintFillArea.transform, false);
        var paintFillImg = paintFillGO.AddComponent<Image>();
        paintFillImg.color = new Color(0.4f, 0.4f, 1f);
        var paintFillRT = paintFillGO.GetComponent<RectTransform>();
        paintFillRT.anchorMin = Vector2.zero;
        paintFillRT.anchorMax = Vector2.one;
        paintFillRT.offsetMin = Vector2.zero;
        paintFillRT.offsetMax = Vector2.zero;
        paintSlider.fillRect = paintFillRT;

        // Paint type text
        var typeText = CreateTMPText(hudRoot, "PaintTypeText", "SÓLIDA [1]",
            new Vector2(0.5f, 0f), new Vector2(260f, 36f), new Vector2(0f, 60f), 20);
        typeText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // HUDManager
        var hud = canvasGO.AddComponent<HUDManager>();
        var hudSO = new SerializedObject(hud);
        hudSO.FindProperty("inkSlider").objectReferenceValue = inkSlider;
        hudSO.FindProperty("inkFill").objectReferenceValue = inkFillImg;
        hudSO.FindProperty("paintPercentText").objectReferenceValue = pctText.GetComponent<TextMeshProUGUI>();
        hudSO.FindProperty("paintSlider").objectReferenceValue = paintSlider;
        hudSO.FindProperty("paintFill").objectReferenceValue = paintFillImg;
        hudSO.FindProperty("paintTypeText").objectReferenceValue = typeText.GetComponent<TextMeshProUGUI>();
        hudSO.FindProperty("hudRoot").objectReferenceValue = hudRoot;
        hudSO.ApplyModifiedProperties();

        return canvasGO;
    }

    static GameObject CreateUICanvas()
    {
        var canvasGO = new GameObject("Canvas_UI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Main Menu
        var menuPanel = CreateFullPanel(canvasGO, "MainMenuPanel", new Color(0f, 0f, 0f, 0.85f));
        CreateTMPText(menuPanel, "Title", "THE PAINTER",
            new Vector2(0.5f, 0.65f), new Vector2(500f, 80f), Vector2.zero, 52);
        var playBtn = CreateButton(menuPanel, "PlayButton", "JUGAR",
            new Vector2(0.5f, 0.4f), new Vector2(0f, -20f));
        var quitBtn = CreateButton(menuPanel, "QuitButton", "SALIR",
            new Vector2(0.5f, 0.4f), new Vector2(0f, -80f));

        // Pause
        var pausePanel = CreateFullPanel(canvasGO, "PausePanel", new Color(0f, 0f, 0f, 0.8f));
        pausePanel.SetActive(false);
        CreateTMPText(pausePanel, "Title", "PAUSA",
            new Vector2(0.5f, 0.65f), new Vector2(300f, 70f), Vector2.zero, 42);
        var resumeBtn = CreateButton(pausePanel, "ResumeButton", "CONTINUAR",
            new Vector2(0.5f, 0.4f), new Vector2(0f, -20f));
        var pauseMenuBtn = CreateButton(pausePanel, "MenuButton", "MENÚ",
            new Vector2(0.5f, 0.4f), new Vector2(0f, -80f));

        // Game Over
        var goPanel = CreateFullPanel(canvasGO, "GameOverPanel", new Color(0.1f, 0f, 0f, 0.9f));
        goPanel.SetActive(false);
        CreateTMPText(goPanel, "Title", "GAME OVER",
            new Vector2(0.5f, 0.65f), new Vector2(400f, 80f), Vector2.zero, 48);
        var retryBtn = CreateButton(goPanel, "RetryButton", "REINTENTAR",
            new Vector2(0.5f, 0.4f), new Vector2(0f, -20f));
        var goMenuBtn = CreateButton(goPanel, "MenuButton", "MENÚ",
            new Vector2(0.5f, 0.4f), new Vector2(0f, -80f));

        // Level Complete
        var completePanel = CreateFullPanel(canvasGO, "LevelCompletePanel",
            new Color(0f, 0.1f, 0f, 0.9f));
        completePanel.SetActive(false);
        CreateTMPText(completePanel, "Title", "¡NIVEL COMPLETO!",
            new Vector2(0.5f, 0.65f), new Vector2(500f, 80f), Vector2.zero, 44);
        var nextBtn = CreateButton(completePanel, "NextButton", "SIGUIENTE",
            new Vector2(0.5f, 0.4f), new Vector2(0f, -20f));
        var completeMenuBtn = CreateButton(completePanel, "MenuButton", "MENÚ",
            new Vector2(0.5f, 0.4f), new Vector2(0f, -80f));

        // UIManager
        var uiManager = canvasGO.AddComponent<UIManager>();
        var uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("mainMenuPanel").objectReferenceValue = menuPanel;
        uiSO.FindProperty("playButton").objectReferenceValue = playBtn.GetComponent<Button>();
        uiSO.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
        uiSO.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        uiSO.FindProperty("resumeButton").objectReferenceValue = resumeBtn.GetComponent<Button>();
        uiSO.FindProperty("pauseMenuButton").objectReferenceValue = pauseMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("gameOverPanel").objectReferenceValue = goPanel;
        uiSO.FindProperty("retryButton").objectReferenceValue = retryBtn.GetComponent<Button>();
        uiSO.FindProperty("gameOverMenuButton").objectReferenceValue = goMenuBtn.GetComponent<Button>();
        uiSO.FindProperty("levelCompletePanel").objectReferenceValue = completePanel;
        uiSO.FindProperty("nextLevelButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        uiSO.FindProperty("completeMenuButton").objectReferenceValue = completeMenuBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        return canvasGO;
    }

    // ── UI Helpers ────────────────────────────────────────────────────────────

    static GameObject CreateFullPanel(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject CreateUIImage(GameObject parent, string name, Color color,
        Vector2 anchor, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return go;
    }

    static GameObject CreateTMPText(GameObject parent, string name, string text,
        Vector2 anchor, Vector2 size, Vector2 pos, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return go;
    }

    static GameObject CreateButton(GameObject parent, string name, string label,
        Vector2 anchor, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.5f, 0.2f);
        go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.sizeDelta = new Vector2(220f, 50f);
        rt.anchoredPosition = pos;

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 22;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        return go;
    }
}
#endif