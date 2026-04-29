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
        EnsureSortingLayer("Background", 0);
        EnsureSortingLayer("Units", 1);

        // ── Caméra ───────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.04f, 0.02f, 0.06f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0f, -0.5f, -10f);
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<UniversalAdditionalCameraData>();
        camGO.AddComponent<CinemachineBrain>();

        var vcamGO = new GameObject("CinemachineCamera");
        vcamGO.transform.position = new Vector3(0f, 0f, -10f);
        vcamGO.AddComponent<CinemachineCamera>();
        var noise = vcamGO.AddComponent<CinemachineBasicMultiChannelPerlin>();
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 1f;

        // ── Lumières ─────────────────────────────────────────────
        var glGO = new GameObject("Global Light 2D");
        var gl2d = glGO.AddComponent<Light2D>();
        gl2d.lightType = Light2D.LightType.Global;
        gl2d.intensity = 0.30f;

        var lanternGO = new GameObject("Lanterne Spirituelle");
        lanternGO.transform.position = new Vector3(0f, -1f, 0f);
        var lantern = lanternGO.AddComponent<Light2D>();
        lantern.lightType = Light2D.LightType.Point;
        lantern.color = new Color(1f, 0.78f, 0.4f);
        lantern.intensity = 1.2f;
        lantern.pointLightOuterRadius = 8f;
        lantern.pointLightInnerRadius = 0.5f;
        lantern.shadowsEnabled = true;
        lantern.shadowIntensity = 0.6f;
        lanternGO.AddComponent<LightFlicker2D>();

        // ── Units ────────────────────────────────────────────────
        Sprite defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var playerGO = new GameObject("PlayerUnit");
        playerGO.transform.position = new Vector3(-4.0f, -1.0f, 0f);
        playerGO.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
        var playerSR = playerGO.AddComponent<SpriteRenderer>();

        // Charger le sprite du joueur
        var playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/player_sprite_sheet_0");
        if (playerSprite == null)
            playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/player_sprite_sheet.png");
        if (playerSprite != null)
        {
            playerSR.sprite = playerSprite;
            playerSR.color = Color.white;
        }
        else
        {
            playerSR.sprite = defaultSprite;
            playerSR.color = new Color(0.55f, 0.45f, 1.00f);
        }
        playerSR.sortingLayerName = "Units";
        playerSR.sortingOrder = 10;

        // Animator joueur
        var playerAnimator = playerGO.AddComponent<Animator>();
        var playerCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Sprites/Player/Player.controller");
        if (playerCtrl != null) playerAnimator.runtimeAnimatorController = playerCtrl;
        var playerUnit = playerGO.AddComponent<BattleUnit>();
        playerUnit.unitName = "Exorciste";
        playerUnit.isPlayer = true;
        playerUnit.maxHP = 100;
        playerUnit.maxMP = 60;
        playerUnit.attack = 15;
        playerUnit.defense = 5;
        playerUnit.speed = 12;
        playerUnit.limitBreakMax = 100;

        var bossGO = new GameObject("BossUnit");
        bossGO.transform.position = new Vector3(3.5f, 0.0f, 0f);
        bossGO.transform.localScale = new Vector3(4.5f, 4.5f, 1f);
        var bossSR = bossGO.AddComponent<SpriteRenderer>();

        // Charger le premier sprite du boss
        var bossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Boss/boss_histoire_battle_0");
        if (bossSprite == null)
            bossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Boss/boss_histoire_battle.png");
        if (bossSprite != null)
        {
            bossSR.sprite = bossSprite;
            bossSR.color = Color.white;
        }
        else
        {
            bossSR.sprite = defaultSprite;
            bossSR.color = new Color(0.70f, 0.06f, 0.06f);
            Debug.LogWarning("[DarkFant] Boss sprite introuvable dans Assets/Sprites/Boss/");
        }
        bossSR.sortingLayerName = "Units";
        bossSR.sortingOrder = 10;

        // Animator du boss
        var bossAnimator = bossGO.AddComponent<Animator>();
        var bossController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Sprites/Boss/Boss.controller");
        if (bossController != null)
            bossAnimator.runtimeAnimatorController = bossController;
        else
            Debug.LogWarning("[DarkFant] Boss.controller introuvable dans Assets/Sprites/Boss/");

        var bossUnit = bossGO.AddComponent<BattleUnit>();
        bossUnit.unitName = "Fléau de Rang S";
        bossUnit.isPlayer = false;
        bossUnit.maxHP = 200;
        bossUnit.attack = 18;
        bossUnit.defense = 8;
        bossUnit.speed = 8;

        // ── EventSystem ──────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // ── Canvas ───────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "Background";
        canvas.sortingOrder = -1;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Background plein écran — render camera pour passer derrière les sprites ───
        string bgPath = "Assets/Sprites/image_battle.jpg";
        var bgImporter = AssetImporter.GetAtPath(bgPath) as TextureImporter;
        if (bgImporter != null && bgImporter.textureType != TextureImporterType.Sprite)
        {
            bgImporter.textureType = TextureImporterType.Sprite;
            bgImporter.spritePixelsPerUnit = 100;
            bgImporter.mipmapEnabled = false;
            bgImporter.filterMode = FilterMode.Point;
            AssetDatabase.ImportAsset(bgPath);
        }
        var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
        var bgGO = MakeGO("BattleBackground", canvasGO.transform);
        bgGO.transform.SetAsFirstSibling();
        var bgImg = bgGO.AddComponent<Image>();
        if (bgSprite != null)
        {
            bgImg.sprite = bgSprite;
            bgImg.preserveAspect = false;
            bgImg.color = Color.white;
        }
        else
        {
            // Pas de sprite trouvé — fond sombre par défaut
            bgImg.color = new Color(0.04f, 0.02f, 0.07f, 1f);
            Debug.LogWarning("[DarkFant] image_battle.jpg introuvable dans Assets/Sprites/ — fond par défaut utilisé.");
        }
        bgImg.raycastTarget = false;
        Stretch(bgGO);

        // Couleurs UI
        Color panelBg = new Color(0.04f, 0.01f, 0.08f, 0.72f);
        Color panelBorder = new Color(0.50f, 0.15f, 0.70f, 1.00f);
        Color bossBorder = new Color(0.75f, 0.08f, 0.08f, 1.00f);

        // ── BOSS HUD — haut centre ────────────────────────────────
        var bossHUDRoot = MakeBP(canvasGO.transform, "BossHUD", panelBg, bossBorder,
            0.20f, 0.87f, 0.80f, 1.00f);
        var bossHUD = bossHUDRoot.AddComponent<BattleHUD>();
        bossHUD.nameText = Label(bossHUDRoot.transform, "BossName", "Fléau de Rang S", 14,
            0.02f, 0.58f, 0.68f, 1.00f);
        bossHUD.nameText.color = new Color(1.0f, 0.55f, 0.55f);
        bossHUD.hpText = Label(bossHUDRoot.transform, "BossHPTxt", "200/200", 11,
            0.68f, 0.58f, 0.98f, 1.00f);
        bossHUD.hpText.alignment = TextAlignmentOptions.MidlineRight;
        bossHUD.hpText.color = new Color(0.85f, 0.85f, 0.85f);
        bossHUD.ghostSlider = Bar(bossHUDRoot.transform, "BossGhost",
            0.02f, 0.06f, 0.98f, 0.56f, new Color(0.60f, 0.04f, 0.04f));
        bossHUD.hpSlider = Bar(bossHUDRoot.transform, "BossHP",
            0.02f, 0.06f, 0.98f, 0.56f, new Color(0.18f, 0.80f, 0.28f));

        // ── TURN ORDER — sous le boss HUD ─────────────────────────
        var turnBarGO = MakeGO("TurnOrderBar", canvasGO.transform);
        Anchors(turnBarGO, 0.32f, 0.81f, 0.68f, 0.88f);
        var toHLG = turnBarGO.AddComponent<HorizontalLayoutGroup>();
        toHLG.spacing = 4f;
        toHLG.childAlignment = TextAnchor.MiddleCenter;
        toHLG.childForceExpandWidth = false;
        toHLG.childForceExpandHeight = false;

        // Prefab icône tour
        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        var tiGO = new GameObject("TurnIconPrefab");
        tiGO.AddComponent<RectTransform>().sizeDelta = new Vector2(26, 26);
        tiGO.AddComponent<Image>().color = Color.white;
        // TextMeshProUGUI crée son propre RectTransform — ne pas utiliser MakeGO
        var tiLbl = new GameObject("Label");
        tiLbl.transform.SetParent(tiGO.transform, false);
        var tiTMP = tiLbl.AddComponent<TextMeshProUGUI>(); // crée RT implicitement
        var tiLblRT = tiLbl.GetComponent<RectTransform>();
        tiLblRT.anchorMin = Vector2.zero; tiLblRT.anchorMax = Vector2.one;
        tiLblRT.offsetMin = tiLblRT.offsetMax = Vector2.zero;
        tiTMP.alignment = TextAlignmentOptions.Center;
        tiTMP.fontSize = 11; tiTMP.fontStyle = FontStyles.Bold; tiTMP.color = Color.white;
        var turnIconAsset = PrefabUtility.SaveAsPrefabAsset(tiGO, "Assets/Prefabs/TurnIconPrefab.prefab");
        Object.DestroyImmediate(tiGO);

        // ── PLAYER HUD — bas gauche ───────────────────────────────
        var playerHUDRoot = MakeBP(canvasGO.transform, "PlayerHUD", panelBg, panelBorder,
            0.01f, 0.01f, 0.37f, 0.28f);
        var playerHUD = playerHUDRoot.AddComponent<BattleHUD>();
        playerHUD.nameText = Label(playerHUDRoot.transform, "PlayerName", "Exorciste", 13,
            0.03f, 0.76f, 1.00f, 1.00f);
        playerHUD.nameText.color = new Color(0.78f, 0.68f, 1.00f);
        // HP
        var hpLbl = Label(playerHUDRoot.transform, "LblHP", "HP", 9, 0.02f, 0.54f, 0.10f, 0.74f);
        hpLbl.color = new Color(0.45f, 1.00f, 0.50f);
        playerHUD.hpText = Label(playerHUDRoot.transform, "PlayerHPTxt", "100/100", 9,
            0.72f, 0.54f, 0.98f, 0.74f);
        playerHUD.hpText.alignment = TextAlignmentOptions.MidlineRight;
        playerHUD.hpText.color = new Color(0.85f, 0.85f, 0.85f);
        playerHUD.ghostSlider = Bar(playerHUDRoot.transform, "PlayerGhost",
            0.11f, 0.54f, 0.98f, 0.74f, new Color(0.60f, 0.04f, 0.04f));
        playerHUD.hpSlider = Bar(playerHUDRoot.transform, "PlayerHP",
            0.11f, 0.54f, 0.98f, 0.74f, new Color(0.18f, 0.80f, 0.28f));
        // MP
        var mpLbl = Label(playerHUDRoot.transform, "LblMP", "MP", 9, 0.02f, 0.30f, 0.10f, 0.52f);
        mpLbl.color = new Color(0.40f, 0.70f, 1.00f);
        playerHUD.mpText = Label(playerHUDRoot.transform, "PlayerMPTxt", "60/60", 9,
            0.72f, 0.30f, 0.98f, 0.52f);
        playerHUD.mpText.alignment = TextAlignmentOptions.MidlineRight;
        playerHUD.mpText.color = new Color(0.55f, 0.80f, 1.00f);
        playerHUD.mpSlider = Bar(playerHUDRoot.transform, "PlayerMP",
            0.11f, 0.30f, 0.98f, 0.52f, new Color(0.15f, 0.45f, 0.90f));
        // Limit Break
        var lbLbl = Label(playerHUDRoot.transform, "LblLB", "LB", 9, 0.02f, 0.05f, 0.10f, 0.28f);
        lbLbl.color = new Color(1.00f, 0.85f, 0.15f);
        playerHUD.limitText = Label(playerHUDRoot.transform, "PlayerLimitTxt", "0/100", 9,
            0.72f, 0.05f, 0.98f, 0.28f);
        playerHUD.limitText.alignment = TextAlignmentOptions.MidlineRight;
        playerHUD.limitText.color = new Color(1.00f, 0.85f, 0.15f);
        playerHUD.limitSlider = Bar(playerHUDRoot.transform, "PlayerLimit",
            0.11f, 0.05f, 0.98f, 0.28f, new Color(0.55f, 0.25f, 0.88f));

        // ── COMMAND PANEL — bas droite ────────────────────────────
        var cmdRoot = MakeBP(canvasGO.transform, "CommandPanel", panelBg, panelBorder,
            0.63f, 0.01f, 0.99f, 0.28f);
        var cmdTitle = Label(cmdRoot.transform, "CmdTitle", "COMMANDES", 11,
            0.05f, 0.80f, 0.95f, 1.00f);
        cmdTitle.color = new Color(0.70f, 0.50f, 1.00f, 0.85f);
        cmdTitle.alignment = TextAlignmentOptions.Center;

        var grid = MakeGO("ButtonGrid", cmdRoot.transform);
        Anchors(grid, 0.02f, 0.02f, 0.98f, 0.78f);
        var gl = grid.AddComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(188, 44);
        gl.spacing = new Vector2(8, 6);
        gl.padding = new RectOffset(4, 4, 4, 4);
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = 2;
        gl.childAlignment = TextAnchor.MiddleCenter;

        Color[] btnColors = {
            new Color(0.38f, 0.08f, 0.60f, 0.92f),
            new Color(0.08f, 0.28f, 0.60f, 0.92f),
            new Color(0.06f, 0.22f, 0.48f, 0.92f),
            new Color(0.52f, 0.26f, 0.04f, 0.92f),
        };
        string[] btnLabels = { "ATTAQUER", "COMPETENCES", "DEFENDRE", "OBJET" };
        var buttons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            var btnGO = MakeGO(btnLabels[i].Trim(), grid.transform);
            btnGO.AddComponent<RectTransform>();
            btnGO.AddComponent<Image>().color = btnColors[i];
            var btn = btnGO.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = btnColors[i];
            cb.highlightedColor = btnColors[i] + new Color(0.20f, 0.20f, 0.20f, 0f);
            cb.pressedColor = btnColors[i] * 0.55f;
            cb.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.45f);
            cb.fadeDuration = 0.07f;
            btn.colors = cb;
            var lbl = new GameObject("Label");
            lbl.transform.SetParent(btnGO.transform, false);
            var tmp = lbl.AddComponent<TextMeshProUGUI>(); // crée RT implicitement
            var lblRT = lbl.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
            tmp.text = btnLabels[i];
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 15; tmp.fontStyle = FontStyles.Bold; tmp.color = Color.white;
            buttons[i] = btn;
        }

        // ── SKILLS PANEL — même zone que CommandPanel ─────────────
        var skillsRoot = MakeBP(canvasGO.transform, "SkillsPanel", panelBg, panelBorder,
            0.63f, 0.01f, 0.99f, 0.28f);
        var skTitle = Label(skillsRoot.transform, "SkillTitle", "COMPÉTENCES", 11,
            0.05f, 0.80f, 0.75f, 1.00f);
        skTitle.color = new Color(0.70f, 0.50f, 1.00f, 0.85f);
        skTitle.alignment = TextAlignmentOptions.Center;

        var skContainer = MakeGO("SkillsContainer", skillsRoot.transform);
        Anchors(skContainer, 0.02f, 0.16f, 0.75f, 0.78f);
        var skHLG = skContainer.AddComponent<HorizontalLayoutGroup>();
        skHLG.spacing = 8f;
        skHLG.childAlignment = TextAnchor.MiddleLeft;
        skHLG.childForceExpandWidth = false;
        skHLG.childForceExpandHeight = true;
        skHLG.padding = new RectOffset(4, 4, 2, 2);

        // Prefab skill button
        var sbGO = new GameObject("SkillButtonPrefab");
        sbGO.AddComponent<RectTransform>().sizeDelta = new Vector2(155, 44);
        sbGO.AddComponent<Image>().color = new Color(0.28f, 0.10f, 0.52f, 0.92f);
        sbGO.AddComponent<Button>();
        var sbLbl = new GameObject("Label");
        sbLbl.transform.SetParent(sbGO.transform, false);
        var sbTMP = sbLbl.AddComponent<TextMeshProUGUI>(); // crée RT implicitement
        var sbLblRT = sbLbl.GetComponent<RectTransform>();
        sbLblRT.anchorMin = Vector2.zero; sbLblRT.anchorMax = Vector2.one;
        sbLblRT.offsetMin = new Vector2(4, 2); sbLblRT.offsetMax = new Vector2(-4, -2);
        sbTMP.alignment = TextAlignmentOptions.Center;
        sbTMP.fontSize = 12; sbTMP.color = Color.white;
        var skillBtnAsset = PrefabUtility.SaveAsPrefabAsset(sbGO, "Assets/Prefabs/SkillButtonPrefab.prefab");
        Object.DestroyImmediate(sbGO);

        // Bouton Retour
        var backRoot = MakeBP(skillsRoot.transform, "BackButton",
            new Color(0.22f, 0.08f, 0.38f, 0.92f), panelBorder,
            0.77f, 0.16f, 0.98f, 0.78f);
        var backBtn = backRoot.AddComponent<Button>();
        var backLbl = Label(backRoot.transform, "BackLabel", "← Retour", 11, 0f, 0f, 1f, 1f);
        backLbl.alignment = TextAlignmentOptions.Center;

        skillsRoot.SetActive(false);

        // ── DIALOGUE BOX — bas, même zone que player HUD + cmd ────
        var dialogueRoot = MakeBP(canvasGO.transform, "DialogueBox", panelBg, panelBorder,
            0.01f, 0.01f, 0.99f, 0.28f);
        var dialogueTMP = Label(dialogueRoot.transform, "Text", "...", 18,
            0.02f, 0.18f, 0.88f, 0.90f);
        dialogueTMP.color = new Color(0.93f, 0.89f, 1.00f);
        dialogueTMP.alignment = TextAlignmentOptions.MidlineLeft;
        var promptTMP = Label(dialogueRoot.transform, "Prompt", "[ ESPACE ]", 10,
            0.84f, 0.04f, 0.98f, 0.18f);
        promptTMP.color = new Color(0.55f, 0.38f, 0.78f, 0.80f);
        promptTMP.alignment = TextAlignmentOptions.MidlineRight;
        dialogueRoot.SetActive(false); // caché par défaut !

        // ── FADE PANEL ────────────────────────────────────────────
        var fadePanelGO = MakeGO("FadePanel", canvasGO.transform);
        Stretch(fadePanelGO);
        fadePanelGO.AddComponent<Image>().color = Color.black;
        var fadeCG = fadePanelGO.AddComponent<CanvasGroup>();
        fadeCG.alpha = 1f;

        // ── COMBAT MANAGER ────────────────────────────────────────
        var cmGO = new GameObject("CombatManager");
        var cm = cmGO.AddComponent<CombatManager>();
        cmGO.AddComponent<BattleFontApplier>();

        cm.playerUnit = playerUnit;
        cm.bossUnit = bossUnit;
        cm.playerTransform = playerGO.transform;
        cm.bossTransform = bossGO.transform;
        cm.playerHUD = playerHUD;
        cm.bossHUD = bossHUD;
        cm.dialogueBox = dialogueRoot;
        cm.dialogueText = dialogueTMP;
        cm.commandPanel = cmdRoot;
        cm.btnAttack = buttons[0];
        cm.btnSkills = buttons[1];
        cm.btnDefend = buttons[2];
        cm.btnItem = buttons[3];
        cm.skillsPanel = skillsRoot;
        cm.skillsContainer = skContainer.transform;
        cm.skillBackButton = backBtn;
        cm.skillButtonPrefab = skillBtnAsset;
        cm.turnOrderContainer = turnBarGO.transform;
        cm.turnIconPrefab = turnIconAsset;
        cm.cameraNoise = noise;
        cm.fadePanel = fadeCG;

        // ── SAVE ──────────────────────────────────────────────────
        string path = "Assets/Scenes/BattleScene.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, path);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity",   true),
            new EditorBuildSettingsScene(path, true),
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DarkFant] BattleScene v4 construite ✓");
    }

    // ── Helpers ───────────────────────────────────────────────────

    static void EnsureSortingLayer(string layerName, int insertAt)
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var sl = tm.FindProperty("m_SortingLayers");
        for (int i = 0; i < sl.arraySize; i++)
            if (sl.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == layerName) return;
        sl.InsertArrayElementAtIndex(Mathf.Clamp(insertAt, 0, sl.arraySize));
        var e = sl.GetArrayElementAtIndex(Mathf.Clamp(insertAt, 0, sl.arraySize - 1));
        e.FindPropertyRelative("name").stringValue = layerName;
        e.FindPropertyRelative("uniqueID").intValue = layerName.GetHashCode();
        tm.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    // Crée un GO avec RectTransform déjà attaché
    static GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    // Panel avec liseré : border → inner panel
    static GameObject MakeBP(Transform parent, string name,
        Color bgColor, Color borderColor, float x0, float y0, float x1, float y1)
    {
        var border = MakeGO(name + "_Border", parent);
        Anchors(border, x0, y0, x1, y1);
        border.AddComponent<Image>().color = borderColor;

        var inner = MakeGO(name, border.transform);
        var rt = inner.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(1, 1); rt.offsetMax = new Vector2(-1, -1);
        inner.AddComponent<Image>().color = bgColor;
        return inner;
    }

    static void Anchors(GameObject go, float x0, float y0, float x1, float y1)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI Label(Transform parent, string name, string text, float size,
        float x0, float y0, float x1, float y1)
    {
        // TextMeshProUGUI ajoute automatiquement un RectTransform — pas de MakeGO ici
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>(); // crée le RectTransform implicitement
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        tmp.text = text; tmp.fontSize = size; tmp.color = Color.white;
        return tmp;
    }

    static Slider Bar(Transform parent, string name,
        float x0, float y0, float x1, float y1, Color fillColor)
    {
        var go = MakeGO(name, parent);
        Anchors(go, x0, y0, x1, y1);
        go.AddComponent<Image>().color = new Color(0.06f, 0.03f, 0.03f, 0.80f);

        var fillArea = MakeGO("Fill Area", go.transform);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.offsetMin = faRT.offsetMax = Vector2.zero;

        var fill = MakeGO("Fill", fillArea.transform);
        var fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
        fRT.offsetMin = fRT.offsetMax = Vector2.zero;
        fill.AddComponent<Image>().color = fillColor;

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fRT;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0; slider.maxValue = 100; slider.value = 100;
        slider.interactable = false;
        return slider;
    }
}