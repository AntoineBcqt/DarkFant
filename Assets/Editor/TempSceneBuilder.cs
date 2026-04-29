using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class TempSceneBuilder
{
    [MenuItem("DarkFant/Build Temp Action Scene")]
    public static void BuildTempScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Caméra ────────────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.06f, 0.04f, 0.10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        camGO.AddComponent<AudioListener>();
        var camFollow = camGO.AddComponent<CameraFollow>();

        // ── Sol ───────────────────────────────────────────────────────────────
        var floorGO = new GameObject("Floor");
        floorGO.AddComponent<SpriteRenderer>().sprite = CreateGridSprite();
        floorGO.GetComponent<SpriteRenderer>().color = new Color(0.12f, 0.08f, 0.18f);
        floorGO.transform.localScale = new Vector3(100f, 100f, 1f);

        // ── Joueur ────────────────────────────────────────────────────────────
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";

        var playerSR = playerGO.AddComponent<SpriteRenderer>();
        playerSR.sortingOrder = 2;
        var playerRB = playerGO.AddComponent<Rigidbody2D>();
        playerRB.gravityScale = 0f;
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        playerGO.AddComponent<CircleCollider2D>().radius = 0.35f;

        var playerAnim = playerGO.AddComponent<Animator>();
        var playerCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Sprites/Player/Player.controller");
        if (playerCtrl != null) playerAnim.runtimeAnimatorController = playerCtrl;

        var pSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/player_sprite_sheet.png");
        if (pSprite != null) { playerSR.sprite = pSprite; playerSR.color = Color.white; }

        var playerCombat = playerGO.AddComponent<PlayerCombat>();
        playerCombat.enemyLayer = LayerMask.GetMask("Enemy");
        playerGO.AddComponent<BonusApplier>();
        camFollow.target = playerGO.transform;

        // ── Prefab Projectile ─────────────────────────────────────────────────
        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        var projGO = new GameObject("OccultProjectile");
        var projSR = projGO.AddComponent<SpriteRenderer>();
        projSR.sprite = CreateCircleSprite(); projSR.color = new Color(0.8f, 0.3f, 1.0f); projSR.sortingOrder = 3;
        projGO.AddComponent<Rigidbody2D>().gravityScale = 0f;
        var projCol = projGO.AddComponent<CircleCollider2D>();
        projCol.radius = 0.15f; projCol.isTrigger = true;
        projGO.AddComponent<OccultProjectile>();
        var projAsset = PrefabUtility.SaveAsPrefabAsset(projGO, "Assets/Prefabs/OccultProjectile.prefab");
        Object.DestroyImmediate(projGO);
        playerCombat.projectilePrefab = projAsset;

        // ── Prefab Ennemi ─────────────────────────────────────────────────────
        var enemyPrefabGO = new GameObject("Enemy");
        enemyPrefabGO.layer = LayerMask.NameToLayer("Enemy");
        var enemySR = enemyPrefabGO.AddComponent<SpriteRenderer>();
        var eSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ennemy/ennemy_sprite_sheet_0.png");
        enemySR.sprite = eSprite != null ? eSprite : CreateCircleSprite();
        enemySR.color = Color.white; enemySR.sortingOrder = 2;
        var enemyRB = enemyPrefabGO.AddComponent<Rigidbody2D>();
        enemyRB.gravityScale = 0f; enemyRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        enemyPrefabGO.AddComponent<CircleCollider2D>().radius = 0.35f;
        enemyPrefabGO.AddComponent<EnemyController>();

        // HP Bar ennemi
        var hpBarGO = new GameObject("HealthBar");
        hpBarGO.transform.SetParent(enemyPrefabGO.transform, false);
        hpBarGO.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        hpBarGO.transform.localScale = new Vector3(0.012f, 0.012f, 1f);
        hpBarGO.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        hpBarGO.GetComponent<RectTransform>().sizeDelta = new Vector2(80f, 10f);
        var hpBGGO = new GameObject("BG"); hpBGGO.transform.SetParent(hpBarGO.transform, false);
        var hpBGRT = hpBGGO.AddComponent<RectTransform>();
        hpBGRT.anchorMin = Vector2.zero; hpBGRT.anchorMax = Vector2.one; hpBGRT.offsetMin = hpBGRT.offsetMax = Vector2.zero;
        hpBGGO.AddComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f, 0.9f);
        var hpFillGO = new GameObject("Fill"); hpFillGO.transform.SetParent(hpBarGO.transform, false);
        var hpFillRT = hpFillGO.AddComponent<RectTransform>();
        hpFillRT.anchorMin = Vector2.zero; hpFillRT.anchorMax = Vector2.one; hpFillRT.offsetMin = hpFillRT.offsetMax = Vector2.zero;
        var hpFillImg = hpFillGO.AddComponent<Image>();
        hpFillImg.color = new Color(0.18f, 0.80f, 0.28f); hpFillImg.type = Image.Type.Filled; hpFillImg.fillMethod = Image.FillMethod.Horizontal;
        hpBarGO.AddComponent<EnemyHealthBar>().fillImage = hpFillImg;

        var enemyAnim = enemyPrefabGO.AddComponent<Animator>();
        var enemyCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Sprites/Ennemy/ennemy_sprite_sheet_14.controller");
        if (enemyCtrl != null) enemyAnim.runtimeAnimatorController = enemyCtrl;
        var enemyAsset = PrefabUtility.SaveAsPrefabAsset(enemyPrefabGO, "Assets/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(enemyPrefabGO);

        // ── Spawner ───────────────────────────────────────────────────────────
        var spawnerGO = new GameObject("EnemySpawner");
        var spawner = spawnerGO.AddComponent<EnemySpawner>();
        spawner.enemyPrefab = enemyAsset; spawner.playerTransform = playerGO.transform;
        spawner.baseEnemiesPerWave = 5; spawner.timeBetweenWaves = 4f;
        spawner.spawnRadius = 8f; spawner.maxEnemiesAlive = 8;

        // ── EventSystem ───────────────────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // ── HUD Canvas ────────────────────────────────────────────────────────
        var canvasGO = new GameObject("HUDCanvas");
        var hudCanvas = canvasGO.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // ── Arena XP System ───────────────────────────────────────────────────
        var xpSystem = new GameObject("ArenaXPSystem").AddComponent<ArenaXPSystem>();

        // ── Player HUD ────────────────────────────────────────────────────────
        var hudGO = new GameObject("PlayerHUD");
        hudGO.transform.SetParent(canvasGO.transform, false);
        SetAnchors(hudGO, 0.01f, 0.88f, 0.30f, 0.99f);
        hudGO.AddComponent<Image>().color = new Color(0.04f, 0.01f, 0.08f, 0.80f);
        var arenaHUD = hudGO.AddComponent<ArenaHUD>();
        arenaHUD.playerCombat = playerCombat;

        // HP Label
        var hpLblGO = MakeTMP(hudGO.transform, "HPLabel", "HP", 12, new Color(0.5f, 1f, 0.55f), TextAlignmentOptions.Left);
        SetAnchorsRT(hpLblGO, 0.02f, 0.62f, 0.40f, 0.95f);

        // HP Bar
        var hpBG = new GameObject("HPBarBG"); hpBG.transform.SetParent(hudGO.transform, false);
        SetAnchors(hpBG, 0.02f, 0.40f, 0.98f, 0.62f);
        hpBG.AddComponent<Image>().color = new Color(0.08f, 0.04f, 0.04f);
        var hpFill = new GameObject("HPBarFill"); hpFill.transform.SetParent(hudGO.transform, false);
        var hpFillRTT = hpFill.AddComponent<RectTransform>();
        SetAnchors(hpFill, 0.02f, 0.40f, 0.98f, 0.62f);
        hpFillRTT.pivot = new Vector2(0f, 0.5f);
        var hpFillI = hpFill.AddComponent<Image>(); hpFillI.color = new Color(0.18f, 0.80f, 0.28f);
        arenaHUD.hpFill = hpFillI;

        // LVL
        var lvlGO = MakeTMP(hudGO.transform, "LevelText", "LVL 1", 11, new Color(0.83f, 0.66f, 1f), TextAlignmentOptions.Right);
        SetAnchorsRT(lvlGO, 0.60f, 0.62f, 0.98f, 0.95f);
        arenaHUD.levelText = lvlGO.GetComponent<TextMeshProUGUI>();

        // XP Bar (fine, sans label)
        var xpBG = new GameObject("XPBarBG"); xpBG.transform.SetParent(hudGO.transform, false);
        SetAnchors(xpBG, 0.02f, 0.08f, 0.98f, 0.20f);
        xpBG.AddComponent<Image>().color = new Color(0.04f, 0.02f, 0.10f);
        var xpFill = new GameObject("XPBarFill"); xpFill.transform.SetParent(hudGO.transform, false);
        var xpFillRT = xpFill.AddComponent<RectTransform>();
        SetAnchors(xpFill, 0.02f, 0.08f, 0.98f, 0.20f);
        xpFillRT.pivot = new Vector2(0f, 0.5f);
        var xpFillI = xpFill.AddComponent<Image>(); xpFillI.color = new Color(0.55f, 0.20f, 1.00f);
        arenaHUD.xpFill = xpFillI;

        // Controls hint
        var hint = MakeTMP(canvasGO.transform, "ControlsHint",
            "ZQSD/Flèches : Déplacer  |  Espace : Dash  |  J/Z : Épée  |  K/X : Tir occulte",
            11, new Color(0.7f, 0.6f, 0.9f, 0.7f), TextAlignmentOptions.Left);
        SetAnchorsRT(hint, 0.01f, 0.01f, 0.70f, 0.06f);

        // ── Level Up Screen ───────────────────────────────────────────────────
        BuildLevelUpScreen(canvasGO, xpSystem);

        // ── Game Over Screen ──────────────────────────────────────────────────
        BuildGameOverScreen(canvasGO, playerCombat);

        // ── Save ──────────────────────────────────────────────────────────────
        string path = "Assets/Scenes/TempActionScene.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DarkFant] TempActionScene construite ✓");
    }

    // ── Level Up Screen ───────────────────────────────────────────────────────

    static void BuildLevelUpScreen(GameObject canvasGO, ArenaXPSystem xpSystem)
    {
        var screenGO = new GameObject("LevelUpScreen");
        screenGO.transform.SetParent(canvasGO.transform, false);
        SetAnchors(screenGO, 0f, 0f, 1f, 1f);
        var cg = screenGO.AddComponent<CanvasGroup>();
        screenGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        var lvlScreen = screenGO.AddComponent<LevelUpScreen>();
        lvlScreen.screenGroup = cg;

        // Titre
        var titleGO = MakeTMP(screenGO.transform, "Title", "NIVEAU SUPÉRIEUR !", 36,
            new Color(1f, 0.85f, 0.20f), TextAlignmentOptions.Center);
        SetAnchorsRT(titleGO, 0.15f, 0.82f, 0.85f, 0.96f);

        var subGO = MakeTMP(screenGO.transform, "Subtitle", "Choisissez un bonus", 14,
            new Color(0.75f, 0.65f, 0.90f), TextAlignmentOptions.Center);
        SetAnchorsRT(subGO, 0.15f, 0.74f, 0.85f, 0.83f);

        // 3 cartes
        float[] xs = { 0.04f, 0.36f, 0.68f };
        for (int i = 0; i < 3; i++)
            lvlScreen.AddCard(BuildBonusCard(screenGO.transform, xs[i], xs[i] + 0.28f));

        screenGO.SetActive(false);
        if (xpSystem != null) xpSystem.levelUpScreen = lvlScreen;
    }

    static BonusCardUI BuildBonusCard(Transform parent, float xMin, float xMax)
    {
        var cardGO = new GameObject("BonusCard");
        cardGO.transform.SetParent(parent, false);
        SetAnchors(cardGO, xMin, 0.08f, xMax, 0.72f);
        var bgImg = cardGO.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.02f, 0.10f, 0.97f);
        var cardUI = cardGO.AddComponent<BonusCardUI>();
        cardUI.cardBG = bgImg;

        // Icône
        var iconGO = new GameObject("Icon"); iconGO.transform.SetParent(cardGO.transform, false);
        SetAnchors(iconGO, 0.20f, 0.68f, 0.80f, 0.96f);
        var iconImg = iconGO.AddComponent<Image>(); iconImg.color = new Color(0.3f, 0.1f, 0.5f, 0.8f);
        cardUI.iconImage = iconImg;

        // Rareté
        var rarGO = MakeTMP(cardGO.transform, "Rarity", "", 9, Color.white, TextAlignmentOptions.Center);
        SetAnchorsRT(rarGO, 0.04f, 0.58f, 0.96f, 0.70f);
        cardUI.rarityLabel = rarGO.GetComponent<TextMeshProUGUI>();

        // Nom
        var nameGO = MakeTMP(cardGO.transform, "Name", "", 14, Color.white, TextAlignmentOptions.Center);
        SetAnchorsRT(nameGO, 0.04f, 0.40f, 0.96f, 0.60f);
        nameGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        cardUI.bonusName = nameGO.GetComponent<TextMeshProUGUI>();

        // Description
        var descGO = MakeTMP(cardGO.transform, "Desc", "", 10, new Color(0.78f, 0.72f, 0.88f), TextAlignmentOptions.Center);
        SetAnchorsRT(descGO, 0.04f, 0.08f, 0.96f, 0.42f);
        descGO.GetComponent<TextMeshProUGUI>().enableWordWrapping = true;
        cardUI.description = descGO.GetComponent<TextMeshProUGUI>();

        return cardUI;
    }

    // ── Game Over Screen ──────────────────────────────────────────────────────

    static void BuildGameOverScreen(GameObject canvasGO, PlayerCombat playerCombat)
    {
        var goGO = new GameObject("GameOverScreen");
        goGO.transform.SetParent(canvasGO.transform, false);
        goGO.SetActive(false);
        SetAnchors(goGO, 0f, 0f, 1f, 1f);
        var cg = goGO.AddComponent<CanvasGroup>();
        goGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

        var titleGO = MakeTMP(goGO.transform, "Title", "GAME OVER", 52,
            new Color(0.85f, 0.08f, 0.08f), TextAlignmentOptions.Center);
        SetAnchorsRT(titleGO, 0.2f, 0.55f, 0.8f, 0.80f);

        var subGO = MakeTMP(goGO.transform, "Subtitle", "L'obscurité vous a consumé...", 16,
            new Color(0.6f, 0.5f, 0.7f), TextAlignmentOptions.Center);
        SetAnchorsRT(subGO, 0.2f, 0.47f, 0.8f, 0.57f);

        var retryGO = CreateMenuButton(goGO.transform, "RetryButton", "RETRY", new Vector2(0.3f, 0.28f), new Vector2(0.5f, 0.38f));
        var quitGO = CreateMenuButton(goGO.transform, "QuitButton", "QUIT", new Vector2(0.5f, 0.28f), new Vector2(0.7f, 0.38f));
        var retryBtn = retryGO.GetComponent<Button>();
        var quitBtn = quitGO.GetComponent<Button>();

        var goScript = goGO.AddComponent<GameOverScreen>();
        goScript.screenGroup = cg;
        goScript.retryButton = retryBtn;
        goScript.quitButton = quitBtn;
        retryBtn.onClick.AddListener(goScript.OnRetry);
        quitBtn.onClick.AddListener(goScript.OnQuit);
        playerCombat.gameOverScreen = goScript;
    }

    static GameObject CreateMenuButton(Transform parent, string name, string label, Vector2 aMin, Vector2 aMax)
    {
        var btnGO = new GameObject(name); btnGO.transform.SetParent(parent, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = aMin; btnRT.anchorMax = aMax; btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
        var btnImg = btnGO.AddComponent<Image>(); btnImg.color = new Color(0.08f, 0.04f, 0.12f, 0.95f);
        var btn = btnGO.AddComponent<Button>();
        var c = btn.colors;
        c.normalColor = new Color(0.08f, 0.04f, 0.12f);
        c.highlightedColor = new Color(0.25f, 0.10f, 0.35f);
        c.pressedColor = new Color(0.50f, 0.10f, 0.10f);
        btn.colors = c;
        var txtGO = MakeTMP(btnGO.transform, "Text", label, 18, new Color(0.85f, 0.75f, 0.95f), TextAlignmentOptions.Center);
        SetAnchorsRT(txtGO, 0f, 0f, 1f, 1f);
        return btnGO;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject MakeTMP(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        return go;
    }

    static void SetAnchors(GameObject go, float x0, float y0, float x1, float y1)
    {
        if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void SetAnchorsRT(GameObject go, float x0, float y0, float x1, float y1)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Sprite CreateCircleSprite() =>
        AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

    static Sprite CreateGridSprite() =>
        AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
}