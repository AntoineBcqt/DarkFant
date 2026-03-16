using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.Rendering.Universal;

public static class BattleSceneBuilder
{
    [MenuItem("DarkFant/Build Battle Scene")]
    public static void BuildBattleScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Sorting Layers ────────────────────────────────────────
        EnsureSortingLayer("Background", 0);
        EnsureSortingLayer("Units",      1);

        // ── Camera principale ─────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.05f, 0.03f, 0.08f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(-0.5f, -0.5f, -10f);
        camGO.AddComponent<AudioListener>();
        var urpData = camGO.AddComponent<UniversalAdditionalCameraData>();

        var brain = camGO.AddComponent<CinemachineBrain>();

        // ── CinemachineCamera fixe (mode Versus) ──────────────────
        var vcamGO = new GameObject("CinemachineCamera");
        vcamGO.transform.position = new Vector3(-0.5f, -0.5f, -10f);
        var vcam = vcamGO.AddComponent<CinemachineCamera>();
        // Pas de Follow/LookAt → caméra fixe
        var noise = vcamGO.AddComponent<CinemachineBasicMultiChannelPerlin>();
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 1f;

        // ── Lumières ──────────────────────────────────────────────
        var globalLightGO = new GameObject("Global Light 2D");
        var globalLight = globalLightGO.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.intensity = 0.25f;
        globalLight.color = Color.white;

        var lanternGO = new GameObject("Lanterne Spirituelle");
        lanternGO.transform.position = new Vector3(-0.5f, -1f, 0f);
        var lantern = lanternGO.AddComponent<Light2D>();
        lantern.lightType = Light2D.LightType.Point;
        lantern.color = new Color(1f, 0.78f, 0.4f);
        lantern.intensity = 1.3f;
        lantern.pointLightOuterRadius = 6f;
        lantern.pointLightInnerRadius = 0.4f;
        lantern.shadowsEnabled = true;
        lantern.shadowIntensity = 0.7f;

        // ── Background world-space (image_battle.jpg) ─────────────
        string bgPath = "Assets/Sprites/image_battle.jpg";
        var bgImporter = AssetImporter.GetAtPath(bgPath) as TextureImporter;
        if (bgImporter != null && bgImporter.textureType != TextureImporterType.Sprite)
        {
            bgImporter.textureType        = TextureImporterType.Sprite;
            bgImporter.spritePixelsPerUnit = 100;
            bgImporter.mipmapEnabled      = false;
            bgImporter.filterMode         = FilterMode.Point;
            AssetDatabase.ImportAsset(bgPath);
        }
        var battleBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);

        var bgWorldGO = new GameObject("BattleBackground");
        bgWorldGO.transform.position = new Vector3(-0.5f, -0.5f, 1f);
        var bgWorldSR = bgWorldGO.AddComponent<SpriteRenderer>();
        bgWorldSR.sprite           = battleBgSprite;
        bgWorldSR.sortingLayerName = "Background";
        bgWorldSR.sortingOrder     = 0;
        if (battleBgSprite != null)
        {
            float camH  = cam.orthographicSize * 2f;
            float camW  = camH * (1280f / 720f);
            float scale = Mathf.Max(camW / battleBgSprite.bounds.size.x, camH / battleBgSprite.bounds.size.y);
            bgWorldGO.transform.localScale = new Vector3(scale, scale, 1f);
        }

        // Sprite carré par défaut (même principe que SampleScene)
        Sprite defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // ── Player (bas gauche) ───────────────────────────────────
        var playerGO = new GameObject("PlayerUnit");
        playerGO.transform.position = new Vector3(-4f, -2.2f, 0f);
        var playerSR = playerGO.AddComponent<SpriteRenderer>();
        playerSR.sprite           = defaultSprite;
        playerSR.color            = new Color(0.50f, 0.42f, 0.95f);
        playerSR.sortingLayerName = "Units";
        playerSR.sortingOrder     = 0;
        var playerHealth = playerGO.AddComponent<HealthSystem>();
        playerHealth.unitName = "Exorciste";
        playerHealth.maxHP = 100;

        // ── Boss (haut droite, imposant) ──────────────────────────
        var bossGO = new GameObject("BossUnit");
        bossGO.transform.position = new Vector3(3.2f, 0.8f, 0f);
        bossGO.transform.localScale = new Vector3(1.9f, 1.9f, 1f);
        var bossSR = bossGO.AddComponent<SpriteRenderer>();
        bossSR.sprite           = defaultSprite;
        bossSR.color            = new Color(0.65f, 0.08f, 0.08f);
        bossSR.sortingLayerName = "Units";
        bossSR.sortingOrder     = 1;
        var bossHealth = bossGO.AddComponent<HealthSystem>();
        bossHealth.unitName = "Fléau de Rang S";
        bossHealth.maxHP = 120;

        // ── EventSystem ───────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // ── Canvas ────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // fond transparent (pour les HUDs qui ont leur propre bg)
        var bgImg = MakeImage(canvasGO.transform, "Background", new Color(0f, 0f, 0f, 0f));
        Stretch(bgImg);

        // ── Boss HUD (haut gauche, compact) ──────────────────────
        var bossHUDGO = MakeImage(canvasGO.transform, "BossHUD", new Color(0.10f, 0.04f, 0.16f, 0.92f));
        Anchors(bossHUDGO, 0.02f, 0.84f, 0.44f, 0.97f);
        var bossHUD = bossHUDGO.AddComponent<BattleHUD>();
        bossHUD.nameText  = MakeLabel(bossHUDGO.transform, "BossName", "Fléau de Rang S", 17, 0.03f, 0.60f, 0.98f, 1.00f);
        bossHUD.ghostSlider = MakeHPBar(bossHUDGO.transform, "BossGhostBar",               0.03f, 0.15f, 0.98f, 0.55f, new Color(0.75f, 0.08f, 0.08f));
        bossHUD.hpSlider  = MakeHPBar(bossHUDGO.transform,  "BossHPBar",                   0.03f, 0.15f, 0.98f, 0.55f);
        bossHUD.hpText    = MakeLabel(bossHUDGO.transform, "BossHP",   "120 / 120",       13, 0.60f, 0.00f, 0.98f, 0.32f);

        // ── Player HUD (bas à droite, près des boutons) ───────────
        var playerHUDGO = MakeImage(canvasGO.transform, "PlayerHUD", new Color(0.10f, 0.04f, 0.16f, 0.92f));
        Anchors(playerHUDGO, 0.52f, 0.52f, 0.98f, 0.65f);
        var playerHUD = playerHUDGO.AddComponent<BattleHUD>();
        playerHUD.nameText  = MakeLabel(playerHUDGO.transform, "PlayerName", "Exorciste", 17, 0.03f, 0.60f, 0.98f, 1.00f);
        playerHUD.ghostSlider = MakeHPBar(playerHUDGO.transform, "PlayerGhostBar",          0.03f, 0.15f, 0.98f, 0.55f, new Color(0.75f, 0.08f, 0.08f));
        playerHUD.hpSlider  = MakeHPBar(playerHUDGO.transform, "PlayerHPBar",               0.03f, 0.15f, 0.98f, 0.55f);
        playerHUD.hpText    = MakeLabel(playerHUDGO.transform, "PlayerHP",  "100 / 100",  13, 0.60f, 0.00f, 0.98f, 0.32f);

        // ── DialogueBox — style identique à SampleScene ───────────
        Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

        var dialogueBox = new GameObject("DialogueBox");
        dialogueBox.transform.SetParent(canvasGO.transform, false);
        var dbRT = dialogueBox.AddComponent<RectTransform>();
        // Même ancrage que SampleScene : stretch full + offset haut pour n'occuper que la bande basse
        dbRT.anchorMin = new Vector2(0f, 0f);
        dbRT.anchorMax = new Vector2(1f, 1f);
        dbRT.offsetMin = Vector2.zero;
        dbRT.offsetMax = new Vector2(0f, -490f);
        var dbImg = dialogueBox.AddComponent<Image>();
        dbImg.sprite = bgSprite;
        dbImg.type   = Image.Type.Sliced;
        dbImg.color  = new Color(1f, 1f, 1f, 0.92f); // même blanc semi-transparent que SampleScene

        var dialogueText = MakeLabel(dialogueBox.transform, "Text (TMP)", "...", 23, 0.03f, 0.12f, 0.95f, 0.92f);
        dialogueText.alignment = TextAlignmentOptions.MidlineLeft;
        dialogueText.color = new Color(0.1f, 0.05f, 0.15f); // texte sombre sur fond clair

        var prompt = MakeLabel(dialogueBox.transform, "Prompt", "[ A / Q ]", 14, 0.78f, 0.02f, 0.99f, 0.28f);
        prompt.color = new Color(0.3f, 0.15f, 0.45f, 0.7f);
        prompt.alignment = TextAlignmentOptions.MidlineRight;

        // ── Command Panel (bas droite, style Persona 5 / Pokémon) ─
        var cmdPanel = MakeImage(canvasGO.transform, "CommandPanel", new Color(0.06f, 0.03f, 0.12f, 0.96f));
        var cpRT = cmdPanel.GetComponent<RectTransform>();
        cpRT.anchorMin = new Vector2(0f, 0f);
        cpRT.anchorMax = new Vector2(1f, 1f);
        cpRT.offsetMin = Vector2.zero;
        cpRT.offsetMax = new Vector2(0f, -490f);

        // Label "COMMANDES"
        var cmdTitle = MakeLabel(cmdPanel.transform, "CmdTitle", "── COMMANDES ──", 16, 0.01f, 0.70f, 0.99f, 1.00f);
        cmdTitle.alignment = TextAlignmentOptions.Center;
        cmdTitle.color = new Color(0.8f, 0.6f, 1f, 0.7f);

        // Grille 2×2 de boutons
        var grid = new GameObject("ButtonGrid");
        grid.transform.SetParent(cmdPanel.transform, false);
        var gRT = grid.AddComponent<RectTransform>();
        gRT.anchorMin = new Vector2(0.01f, 0.02f);
        gRT.anchorMax = new Vector2(0.99f, 0.68f);
        gRT.offsetMin = gRT.offsetMax = Vector2.zero;
        var gl = grid.AddComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(280, 54);
        gl.spacing = new Vector2(10, 8);
        gl.padding = new RectOffset(6, 6, 4, 4);
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = 2;
        gl.childAlignment = TextAnchor.MiddleCenter;

        // Couleurs des boutons
        Color[] btnColors = {
            new Color(0.42f, 0.10f, 0.62f), // Attaquer  — violet
            new Color(0.10f, 0.32f, 0.62f), // Défendre  — bleu
            new Color(0.62f, 0.36f, 0.06f), // Lanterne  — orange
            new Color(0.30f, 0.30f, 0.30f), // Éveil     — gris (disabled)
        };
        string[] btnLabels = { "⚔  Attaquer", "🛡  Défendre", "🕯  Lanterne Sacrée", "✦  Éveil" };

        var buttons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            var btnGO = new GameObject(btnLabels[i].Trim());
            btnGO.transform.SetParent(grid.transform, false);
            btnGO.AddComponent<RectTransform>();
            var img = btnGO.AddComponent<Image>();
            img.color = btnColors[i];
            var btn = btnGO.AddComponent<Button>();

            var cb = btn.colors;
            cb.normalColor      = btnColors[i];
            cb.highlightedColor = btnColors[i] + new Color(0.28f, 0.28f, 0.28f, 0f); // très visible à la manette
            cb.selectedColor    = btnColors[i] + new Color(0.28f, 0.28f, 0.28f, 0f);
            cb.pressedColor     = btnColors[i] * 0.65f;
            cb.disabledColor    = new Color(0.20f, 0.20f, 0.20f, 0.55f);
            cb.colorMultiplier  = 1f;
            cb.fadeDuration     = 0.08f; // transition rapide
            btn.colors = cb;
            btn.navigation = new Navigation { mode = Navigation.Mode.Explicit }; // navigation manette configurée manuellement

            if (i == 3) btn.interactable = false; // Éveil grisé par défaut

            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(btnGO.transform, false);
            var lblRT = lblGO.AddComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text = btnLabels[i];
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontSize = 20;
            lbl.fontStyle = FontStyles.Bold;
            lbl.color = Color.white;

            buttons[i] = btn;
        }

        // ── Fade Panel (noir, pour fondu au noir) ─────────────────
        var fadePanelGO = MakeImage(canvasGO.transform, "FadePanel", Color.black);
        Stretch(fadePanelGO);
        var fadeCG = fadePanelGO.AddComponent<CanvasGroup>();
        fadeCG.alpha = 1f;

        // ── CombatManager ─────────────────────────────────────────
        var cmGO = new GameObject("CombatManager");
        var cm = cmGO.AddComponent<CombatManager>();

        cm.playerHealth     = playerHealth;
        cm.bossHealth       = bossHealth;
        cm.playerTransform  = playerGO.transform;
        cm.bossTransform    = bossGO.transform;
        cm.playerHUD        = playerHUD;
        cm.bossHUD          = bossHUD;
        cm.dialogueBox      = dialogueBox;
        cm.dialogueText     = dialogueText;
        cm.commandPanel     = cmdPanel;
        cm.commandPanelBg   = cmdPanel.GetComponent<Image>();
        cm.btnAttack        = buttons[0];
        cm.btnDefend        = buttons[1];
        cm.btnLantern       = buttons[2];
        cm.btnAwakening     = buttons[3];
        cm.cameraNoise      = noise;
        cm.fadePanel        = fadeCG;

        // ── Save ──────────────────────────────────────────────────
        string path = "Assets/Scenes/BattleScene.unity";
        EditorSceneManager.SaveScene(scene, path);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
            new EditorBuildSettingsScene(path, true),
        };

        Debug.Log("[DarkFant] BattleScene side-view reconstruite !");
    }

    // ── Helpers ───────────────────────────────────────────────────

    static void EnsureSortingLayer(string layerName, int insertAt)
    {
        var tagManager    = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var sortingLayers = tagManager.FindProperty("m_SortingLayers");

        for (int i = 0; i < sortingLayers.arraySize; i++)
            if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == layerName) return;

        sortingLayers.InsertArrayElementAtIndex(Mathf.Clamp(insertAt, 0, sortingLayers.arraySize));
        var entry = sortingLayers.GetArrayElementAtIndex(Mathf.Clamp(insertAt, 0, sortingLayers.arraySize - 1));
        entry.FindPropertyRelative("name").stringValue     = layerName;
        entry.FindPropertyRelative("uniqueID").intValue    = layerName.GetHashCode();
        tagManager.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    static GameObject MakeImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = color;
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Anchors(GameObject go, float x0, float y0, float x1, float y1)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI MakeLabel(Transform parent, string name, string text, float size,
        float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = Color.white;
        return tmp;
    }

    static Slider MakeHPBar(Transform parent, string name, float x0, float y0, float x1, float y1,
        Color? fillColor = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = new Vector2(4, 2); rt.offsetMax = new Vector2(-4, -2);
        go.AddComponent<Image>().color = new Color(0.10f, 0.05f, 0.05f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.offsetMin = faRT.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fRT = fill.AddComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
        fRT.offsetMin = fRT.offsetMax = Vector2.zero;
        fill.AddComponent<Image>().color = fillColor ?? new Color(0.18f, 0.80f, 0.28f);

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fRT;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0; slider.maxValue = 100; slider.value = 100;
        slider.interactable = false;
        return slider;
    }
}
