using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Builder de la scène temporaire d'action top-down style Hadès.
/// Menu : DarkFant → Build Temp Action Scene
/// </summary>
public static class TempSceneBuilder
{
    [MenuItem("DarkFant/Build Temp Action Scene")]
    public static void BuildTempScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Caméra ───────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.06f, 0.04f, 0.10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        camGO.AddComponent<AudioListener>();

        // Caméra suit le joueur
        var camFollow = camGO.AddComponent<CameraFollow>();

        // ── Sol (fond sombre avec grille) ─────────────────────────
        var floorGO = new GameObject("Floor");
        var floorSR = floorGO.AddComponent<SpriteRenderer>();
        floorSR.sprite = CreateGridSprite();
        floorSR.color = new Color(0.12f, 0.08f, 0.18f);
        floorGO.transform.localScale = new Vector3(100f, 100f, 1f);

        // ── Joueur ────────────────────────────────────────────────
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.transform.position = Vector3.zero;

        var playerSR = playerGO.AddComponent<SpriteRenderer>();
        playerSR.sprite = CreateCircleSprite(0.4f);
        playerSR.color = new Color(0.55f, 0.45f, 1.00f);
        playerSR.sortingOrder = 2;

        var playerRB = playerGO.AddComponent<Rigidbody2D>();
        playerRB.gravityScale = 0f;
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var playerCol = playerGO.AddComponent<CircleCollider2D>();
        playerCol.radius = 0.35f;

        // Ajouter l'Animator sur le joueur
        var playerAnimator = playerGO.AddComponent<Animator>();
        var playerController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Sprites/Player/Player.controller");
        if (playerController != null)
            playerAnimator.runtimeAnimatorController = playerController;
        else
            Debug.LogWarning("[DarkFant] Player.controller introuvable dans Assets/Sprites/Player/");

        // Charger le sprite idle du joueur
        var playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/player_sprite_sheet_0");
        if (playerSprite == null)
            playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/player_sprite_sheet.png");
        if (playerSprite != null) playerSR.sprite = playerSprite;
        playerSR.color = Color.white;

        var playerCombat = playerGO.AddComponent<PlayerCombat>();
        playerCombat.enemyLayer = LayerMask.GetMask("Enemy");

        // Assigner la caméra follow au joueur
        camFollow.target = playerGO.transform;

        // ── Prefab Projectile ─────────────────────────────────────
        System.IO.Directory.CreateDirectory("Assets/Prefabs");

        var projGO = new GameObject("OccultProjectile");
        var projSR = projGO.AddComponent<SpriteRenderer>();
        projSR.sprite = CreateCircleSprite(0.18f);
        projSR.color = new Color(0.8f, 0.3f, 1.0f);
        projSR.sortingOrder = 3;
        var projRB = projGO.AddComponent<Rigidbody2D>();
        projRB.gravityScale = 0f;
        var projCol = projGO.AddComponent<CircleCollider2D>();
        projCol.radius = 0.15f;
        projCol.isTrigger = true;
        projGO.layer = LayerMask.NameToLayer("Default");
        projGO.AddComponent<OccultProjectile>();
        var projAsset = PrefabUtility.SaveAsPrefabAsset(projGO, "Assets/Prefabs/OccultProjectile.prefab");
        Object.DestroyImmediate(projGO);
        playerCombat.projectilePrefab = projAsset;

        // ── Prefab Ennemi ─────────────────────────────────────────
        var enemyPrefabGO = new GameObject("Enemy");
        enemyPrefabGO.layer = LayerMask.NameToLayer("Enemy");

        var enemySR = enemyPrefabGO.AddComponent<SpriteRenderer>();
        // Charger le premier sprite de la sheet si disponible
        var enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ennemy_sprite_sheet_0.png");
        if (enemySprite == null)
            enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ennemy/ennemy_sprite_sheet_0.png");
        enemySR.sprite = enemySprite != null ? enemySprite : CreateCircleSprite(0.4f);
        enemySR.color = Color.white; // pas de teinte colorée
        enemySR.sortingOrder = 2;

        var enemyRB = enemyPrefabGO.AddComponent<Rigidbody2D>();
        enemyRB.gravityScale = 0f;
        enemyRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        var enemyCol = enemyPrefabGO.AddComponent<CircleCollider2D>();
        enemyCol.radius = 0.35f;

        enemyPrefabGO.AddComponent<EnemyController>();

        // Barre de vie flottante
        var hpBarGO = new GameObject("HealthBar");
        hpBarGO.transform.SetParent(enemyPrefabGO.transform, false);
        hpBarGO.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        hpBarGO.transform.localScale = new Vector3(0.012f, 0.012f, 1f);

        var canvas = hpBarGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rt = hpBarGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80f, 10f);

        // Fond barre
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(hpBarGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        bgGO.AddComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f, 0.9f);

        // Fill barre
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(hpBarGO.transform, false);
        var fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.18f, 0.80f, 0.28f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;

        var hpBar = hpBarGO.AddComponent<EnemyHealthBar>();
        hpBar.fillImage = fillImg;

        // Ajouter l'Animator sur le prefab ennemi
        var enemyAnimator = enemyPrefabGO.AddComponent<Animator>();
        // Charger le controller si déjà créé
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Sprites/Ennemy/ennemy_sprite_sheet_14.controller");
        if (controller != null)
            enemyAnimator.runtimeAnimatorController = controller;
        else
            Debug.LogWarning("[DarkFant] ennemy_sprite_sheet_14.controller introuvable — assigne-le manuellement sur le prefab.");

        var enemyAsset = PrefabUtility.SaveAsPrefabAsset(enemyPrefabGO, "Assets/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(enemyPrefabGO);

        // ── Spawner ───────────────────────────────────────────────
        var spawnerGO = new GameObject("EnemySpawner");
        var spawner = spawnerGO.AddComponent<EnemySpawner>();
        spawner.enemyPrefab = enemyAsset;
        spawner.playerTransform = playerGO.transform;
        spawner.enemiesPerWave = 3;
        spawner.timeBetweenWaves = 4f;
        spawner.spawnRadius = 8f;
        spawner.maxEnemiesAlive = 8;

        // ── EventSystem ──────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // ── HUD Joueur (Canvas) ───────────────────────────────────
        var canvasGO = new GameObject("HUDCanvas");
        var hudCanvas = canvasGO.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // HP Bar joueur (bas gauche)
        var playerHUDGO = new GameObject("PlayerHUD");
        playerHUDGO.transform.SetParent(canvasGO.transform, false);
        playerHUDGO.AddComponent<RectTransform>();
        SetAnchors(playerHUDGO, 0.01f, 0.88f, 0.28f, 0.99f);
        playerHUDGO.AddComponent<Image>().color = new Color(0.04f, 0.01f, 0.08f, 0.80f);

        var playerHUDBar = new GameObject("PlayerHUDBar");
        playerHUDBar.transform.SetParent(canvasGO.transform, false);
        playerHUDBar.AddComponent<RectTransform>();
        SetAnchors(playerHUDBar, 0.01f, 0.88f, 0.28f, 0.99f);

        // Ajouter le PlayerHUD script
        var playerHUDScript = playerHUDGO.AddComponent<PlayerHUD>();
        playerHUDScript.playerCombat = playerCombat;

        // HP label
        var hpLabelGO = new GameObject("HPLabel");
        hpLabelGO.transform.SetParent(playerHUDGO.transform, false);
        var hpLabelTMP = hpLabelGO.AddComponent<TextMeshProUGUI>();
        var hpLabelRT = hpLabelGO.GetComponent<RectTransform>();
        hpLabelRT.anchorMin = new Vector2(0.02f, 0.5f);
        hpLabelRT.anchorMax = new Vector2(0.98f, 0.98f);
        hpLabelRT.offsetMin = hpLabelRT.offsetMax = Vector2.zero;
        hpLabelTMP.text = "HP";
        hpLabelTMP.fontSize = 14;
        hpLabelTMP.color = new Color(0.50f, 1.00f, 0.55f);

        // HP fill
        var hpFillBG = new GameObject("HPBarBG");
        hpFillBG.transform.SetParent(playerHUDGO.transform, false);
        hpFillBG.AddComponent<RectTransform>();
        SetAnchors(hpFillBG, 0.02f, 0.05f, 0.98f, 0.48f);
        hpFillBG.AddComponent<Image>().color = new Color(0.08f, 0.04f, 0.04f);

        var hpFillGO = new GameObject("HPBarFill");
        hpFillGO.transform.SetParent(playerHUDGO.transform, false);
        hpFillGO.AddComponent<RectTransform>();
        SetAnchors(hpFillGO, 0.02f, 0.05f, 0.98f, 0.48f);
        var hpFillImg = hpFillGO.AddComponent<Image>();
        hpFillImg.color = new Color(0.18f, 0.80f, 0.28f);
        hpFillImg.type = Image.Type.Filled;
        hpFillImg.fillMethod = Image.FillMethod.Horizontal;
        playerHUDScript.hpFill = hpFillImg;

        // Controls hint
        var hintsGO = new GameObject("ControlsHint");
        hintsGO.transform.SetParent(canvasGO.transform, false);
        var hintsTMP = hintsGO.AddComponent<TextMeshProUGUI>();
        var hintsRT = hintsGO.GetComponent<RectTransform>();
        hintsRT.anchorMin = new Vector2(0.01f, 0.01f);
        hintsRT.anchorMax = new Vector2(0.50f, 0.12f);
        hintsRT.offsetMin = hintsRT.offsetMax = Vector2.zero;
        hintsTMP.text = "ZQSD/Flèches : Déplacer  |  Espace : Dash  |  J/Z : Épée  |  K/X : Tir occulte";
        hintsTMP.fontSize = 11;
        hintsTMP.color = new Color(0.7f, 0.6f, 0.9f, 0.7f);

        // ── Game Over Screen ──────────────────────────────────────
        BuildGameOverScreen(canvasGO, playerCombat);

        // ── Save ──────────────────────────────────────────────────
        string path = "Assets/Scenes/TempActionScene.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DarkFant] TempActionScene construite ✓  |  ZQSD + Espace + J + K");
    }

    // ── Game Over Screen Builder ──────────────────────────────────

    static void BuildGameOverScreen(GameObject canvasGO, PlayerCombat playerCombat)
    {
        var goGO = new GameObject("GameOverScreen");
        goGO.transform.SetParent(canvasGO.transform, false);
        goGO.SetActive(false);

        var goRT = goGO.AddComponent<RectTransform>();
        goRT.anchorMin = Vector2.zero;
        goRT.anchorMax = Vector2.one;
        goRT.offsetMin = goRT.offsetMax = Vector2.zero;

        var cg = goGO.AddComponent<CanvasGroup>();

        var bg = goGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.88f);

        // Titre GAME OVER
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(goGO.transform, false);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.2f, 0.55f);
        titleRT.anchorMax = new Vector2(0.8f, 0.80f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;
        titleTMP.text = "GAME OVER";
        titleTMP.fontSize = 52;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(0.85f, 0.08f, 0.08f);

        // Sous-titre
        var subGO = new GameObject("Subtitle");
        subGO.transform.SetParent(goGO.transform, false);
        var subTMP = subGO.AddComponent<TextMeshProUGUI>();
        var subRT = subGO.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0.2f, 0.47f);
        subRT.anchorMax = new Vector2(0.8f, 0.57f);
        subRT.offsetMin = subRT.offsetMax = Vector2.zero;
        subTMP.text = "L'obscurité vous a consumé...";
        subTMP.fontSize = 16;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.color = new Color(0.6f, 0.5f, 0.7f);

        // Bouton RETRY
        var retryGO = CreateMenuButton(goGO.transform, "RetryButton", "RETRY", new Vector2(0.3f, 0.28f), new Vector2(0.5f, 0.38f));
        var retryBtn = retryGO.GetComponent<Button>();

        // Bouton QUIT
        var quitGO = CreateMenuButton(goGO.transform, "QuitButton", "QUIT", new Vector2(0.5f, 0.28f), new Vector2(0.7f, 0.38f));
        var quitBtn = quitGO.GetComponent<Button>();

        var goScript = goGO.AddComponent<GameOverScreen>();
        goScript.screenGroup = cg;
        goScript.retryButton = retryBtn;
        goScript.quitButton = quitBtn;

        retryBtn.onClick.AddListener(goScript.OnRetry);
        quitBtn.onClick.AddListener(goScript.OnQuit);

        playerCombat.gameOverScreen = goScript;
    }

    static GameObject CreateMenuButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        var btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = anchorMin;
        btnRT.anchorMax = anchorMax;
        btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;

        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.08f, 0.04f, 0.12f, 0.95f);

        var btn = btnGO.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0.08f, 0.04f, 0.12f);
        colors.highlightedColor = new Color(0.25f, 0.10f, 0.35f);
        colors.pressedColor = new Color(0.50f, 0.10f, 0.10f);
        btn.colors = colors;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        var txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;
        txt.text = label;
        txt.fontSize = 18;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = new Color(0.85f, 0.75f, 0.95f);

        return btnGO;
    }

    // ── Helpers ───────────────────────────────────────────────────

    static void SetAnchors(GameObject go, float x0, float y0, float x1, float y1)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Sprite CreateCircleSprite(float radius = 0.5f)
    {
        // Sprite carré blanc Unity par défaut — remplacé par vos sprites plus tard
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    static Sprite CreateGridSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
    }
}