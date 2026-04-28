using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Écran d'intro Arena — lore + règles + transition vers TempActionScene.
/// Scène dédiée : ArenaIntroScene
/// </summary>
public class ArenaIntroController : MonoBehaviour
{
    [Header("Scène")]
    [SerializeField] private string arenaSceneName = "TempActionScene";
    [SerializeField] private float  transitionDuration = 0.8f;

    [Header("Font")]
    [SerializeField] private Font pixelFont;

    private UIDocument    _uiDocument;
    private VisualElement _root;
    private VisualElement _starsLayer;
    private VisualElement[] _stars;
    private readonly int _starCount = 60;

    private void Awake() => _uiDocument = GetComponent<UIDocument>();

    private void Start()
    {
        var root = _uiDocument.rootVisualElement;
        _root = BuildUI(root);
        if (pixelFont != null) ApplyFont(_root, pixelFont);
        GenerateStars();
        StartCoroutine(AnimateStars());
        StartCoroutine(FadeIn());
    }

    private VisualElement BuildUI(VisualElement root)
    {
        // Root container
        var container = new VisualElement();
        container.name = "root";
        container.style.width            = new Length(100, LengthUnit.Percent);
        container.style.height           = new Length(100, LengthUnit.Percent);
        container.style.backgroundColor  = new Color(0.04f, 0.02f, 0.08f);
        container.style.alignItems       = Align.Center;
        container.style.justifyContent   = Justify.Center;
        container.style.flexDirection    = FlexDirection.Column;
        container.style.opacity          = 0f;

        // Étoiles
        _starsLayer = new VisualElement();
        _starsLayer.name = "stars-layer";
        _starsLayer.style.position = Position.Absolute;
        _starsLayer.style.top = 0; _starsLayer.style.left = 0;
        _starsLayer.style.width  = new Length(100, LengthUnit.Percent);
        _starsLayer.style.height = new Length(100, LengthUnit.Percent);
        container.Add(_starsLayer);

        // Titre ARENA
        var title = new Label("⚔ ARENA ⚔");
        title.style.fontSize       = 42;
        title.style.color          = new Color(0.85f, 0.08f, 0.20f);
        title.style.marginBottom   = 8;
        title.style.unityTextAlign = TextAnchor.MiddleCenter;
        container.Add(title);

        // Sous-titre lore
        var lore = new Label("Les portes de l'Enfer se sont ouvertes.\nDes créatures des ténèbres déferlent sans fin.\nTu es le dernier rempart contre l'obscurité.");
        lore.style.fontSize        = 13;
        lore.style.color           = new Color(0.65f, 0.55f, 0.80f);
        lore.style.marginBottom    = 32;
        lore.style.unityTextAlign  = TextAnchor.MiddleCenter;
        lore.style.whiteSpace      = WhiteSpace.Normal;
        lore.style.maxWidth        = new Length(60, LengthUnit.Percent);
        container.Add(lore);

        // Séparateur
        var sep = new VisualElement();
        sep.style.width           = new Length(40, LengthUnit.Percent);
        sep.style.height          = 1;
        sep.style.backgroundColor = new Color(0.30f, 0.10f, 0.50f);
        sep.style.marginBottom    = 28;
        container.Add(sep);

        // Règles - cadre
        var rulesFrame = new VisualElement();
        rulesFrame.style.flexDirection    = FlexDirection.Column;
        rulesFrame.style.alignItems       = Align.Center;
        rulesFrame.style.paddingTop       = 20; rulesFrame.style.paddingBottom = 20;
        rulesFrame.style.paddingLeft      = 36; rulesFrame.style.paddingRight  = 36;
        rulesFrame.style.borderTopWidth   = 1; rulesFrame.style.borderBottomWidth = 1;
        rulesFrame.style.borderLeftWidth  = 1; rulesFrame.style.borderRightWidth  = 1;
        rulesFrame.style.borderTopColor   = new Color(0.25f, 0.10f, 0.45f);
        rulesFrame.style.borderBottomColor = new Color(0.25f, 0.10f, 0.45f);
        rulesFrame.style.borderLeftColor  = new Color(0.25f, 0.10f, 0.45f);
        rulesFrame.style.borderRightColor = new Color(0.25f, 0.10f, 0.45f);
        rulesFrame.style.backgroundColor  = new Color(0.06f, 0.02f, 0.12f, 0.8f);
        rulesFrame.style.marginBottom     = 36;
        container.Add(rulesFrame);

        var rulesTitle = new Label("— RÈGLES —");
        rulesTitle.style.fontSize      = 11;
        rulesTitle.style.color         = new Color(0.83f, 0.66f, 1f);
        rulesTitle.style.marginBottom  = 14;
        rulesTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
        rulesFrame.Add(rulesTitle);

        string[] rules = {
            "⚔  Tue les ennemis pour gagner de l'XP",
            "★  Monte de niveau pour choisir un bonus",
            "♦  Les bonus ont des raretés : Commun → Occulte",
            "☠  Survive le plus longtemps possible",
            "⚡  Un boss apparaît tous les 10 niveaux",
        };

        string[] controls = {
            "ZQSD / Flèches  →  Se déplacer",
            "Espace          →  Dash (invincible)",
            "J / Z           →  Attaque épée",
            "K / X           →  Tir occulte",
        };

        foreach (var rule in rules)
        {
            var lbl = new Label(rule);
            lbl.style.fontSize      = 11;
            lbl.style.color         = new Color(0.75f, 0.70f, 0.88f);
            lbl.style.marginBottom  = 6;
            lbl.style.unityTextAlign = TextAnchor.MiddleLeft;
            rulesFrame.Add(lbl);
        }

        var sep2 = new VisualElement();
        sep2.style.width           = new Length(90, LengthUnit.Percent);
        sep2.style.height          = 1;
        sep2.style.backgroundColor = new Color(0.20f, 0.08f, 0.35f);
        sep2.style.marginTop       = 10; sep2.style.marginBottom = 10;
        rulesFrame.Add(sep2);

        foreach (var ctrl in controls)
        {
            var lbl = new Label(ctrl);
            lbl.style.fontSize      = 11;
            lbl.style.color         = new Color(0.60f, 0.55f, 0.75f);
            lbl.style.marginBottom  = 5;
            lbl.style.unityTextAlign = TextAnchor.MiddleLeft;
            rulesFrame.Add(lbl);
        }

        // Bouton ENTRER EN ARÈNE
        var btn = new Button();
        btn.text                    = "ENTRER EN ARÈNE";
        btn.style.width             = 280;
        btn.style.height            = 52;
        btn.style.fontSize          = 14;
        btn.style.backgroundColor   = new Color(0.08f, 0.02f, 0.16f);
        btn.style.borderTopColor    = new Color(0.75f, 0.10f, 0.20f);
        btn.style.borderBottomColor = new Color(0.75f, 0.10f, 0.20f);
        btn.style.borderLeftColor   = new Color(0.75f, 0.10f, 0.20f);
        btn.style.borderRightColor  = new Color(0.75f, 0.10f, 0.20f);
        btn.style.borderTopWidth    = 2; btn.style.borderBottomWidth = 2;
        btn.style.borderLeftWidth   = 2; btn.style.borderRightWidth  = 2;
        btn.style.color             = new Color(1f, 0.85f, 0.90f);
        btn.style.unityTextAlign    = TextAnchor.MiddleCenter;
        btn.style.marginBottom      = 12;
        btn.RegisterCallback<ClickEvent>(_ => StartCoroutine(TransitionTo(arenaSceneName)));
        container.Add(btn);

        // Bouton RETOUR
        var btnBack = new Button();
        btnBack.text                    = "< RETOUR";
        btnBack.style.width             = 180;
        btnBack.style.height            = 36;
        btnBack.style.fontSize          = 11;
        btnBack.style.backgroundColor   = new Color(0f, 0f, 0f, 0f);
        btnBack.style.borderTopColor    = new Color(0.30f, 0.25f, 0.40f);
        btnBack.style.borderBottomColor = new Color(0.30f, 0.25f, 0.40f);
        btnBack.style.borderLeftColor   = new Color(0.30f, 0.25f, 0.40f);
        btnBack.style.borderRightColor  = new Color(0.30f, 0.25f, 0.40f);
        btnBack.style.borderTopWidth    = 1; btnBack.style.borderBottomWidth = 1;
        btnBack.style.borderLeftWidth   = 1; btnBack.style.borderRightWidth  = 1;
        btnBack.style.color             = new Color(0.55f, 0.50f, 0.65f);
        btnBack.style.unityTextAlign    = TextAnchor.MiddleCenter;
        btnBack.RegisterCallback<ClickEvent>(_ => StartCoroutine(TransitionTo("MainMenuScene")));
        container.Add(btnBack);

        root.Add(container);
        return container;
    }

    // ── Transition ────────────────────────────────────────────────────────────

    private IEnumerator TransitionTo(string sceneName)
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            if (_root != null) _root.style.opacity = 1f - Mathf.Clamp01(t / transitionDuration);
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }

    // ── Fade In ───────────────────────────────────────────────────────────────

    private IEnumerator FadeIn()
    {
        float t = 0f;
        if (_root != null) _root.style.opacity = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            if (_root != null) _root.style.opacity = Mathf.Clamp01(t);
            yield return null;
        }
        if (_root != null) _root.style.opacity = 1f;
    }

    // ── Étoiles ───────────────────────────────────────────────────────────────

    private void GenerateStars()
    {
        if (_starsLayer == null) return;
        _stars = new VisualElement[_starCount];
        for (int i = 0; i < _starCount; i++)
        {
            var star = new VisualElement();
            star.style.position        = Position.Absolute;
            star.style.width           = Random.Range(1, 3);
            star.style.height          = star.style.width;
            star.style.backgroundColor = new Color(1f, 1f, 1f, 0f);
            star.style.left            = new Length(Random.Range(0f, 100f), LengthUnit.Percent);
            star.style.top             = new Length(Random.Range(0f, 100f), LengthUnit.Percent);
            _starsLayer.Add(star);
            _stars[i] = star;
        }
    }

    private IEnumerator AnimateStars()
    {
        if (_stars == null) yield break;
        float[] phases = new float[_starCount], speeds = new float[_starCount], maxAlphas = new float[_starCount];
        for (int i = 0; i < _starCount; i++)
        {
            phases[i]    = Random.Range(0f, Mathf.PI * 2f);
            speeds[i]    = Random.Range(0.4f, 1.8f);
            maxAlphas[i] = Random.Range(0.2f, 0.85f);
        }
        while (true)
        {
            float t = Time.time;
            for (int i = 0; i < _starCount; i++)
            {
                if (_stars[i] == null) continue;
                float alpha = (Mathf.Sin(t * speeds[i] + phases[i]) + 1f) / 2f * maxAlphas[i];
                _stars[i].style.backgroundColor = new Color(1f, 1f, 1f, alpha);
            }
            yield return null;
        }
    }

    private void ApplyFont(VisualElement el, Font font)
    {
        var fd = FontDefinition.FromFont(font);
        if (el is Label l)  l.style.unityFontDefinition = new StyleFontDefinition(fd);
        if (el is Button b) b.style.unityFontDefinition = new StyleFontDefinition(fd);
        foreach (var child in el.Children()) ApplyFont(child, font);
    }
}
