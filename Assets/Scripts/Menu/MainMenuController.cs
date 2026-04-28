using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string histoireSceneName = "ExploreScene";
    [SerializeField] private string arenaSceneName = "ArenaIntroScene";
    [SerializeField] private float transitionDuration = 0.6f;

    [Header("Font")]
    [SerializeField] private Font pixelFont;

    [Header("References")]
    [SerializeField] private OptionsPanelController optionsPanel;

    private UIDocument _uiDocument;
    private MainMenuAudio _audio;

    private Button _btnStart;
    private Button _btnOptions;
    private VisualElement _menuContainer; // contient title + frame
    private VisualElement _subMenu;
    private VisualElement _root;
    private VisualElement _starsLayer;

    private readonly int _starCount = 30;
    private VisualElement[] _stars;
    private bool _isAnimating = false;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _audio = GetComponent<MainMenuAudio>();
    }

    private void Start()
    {
        if (_uiDocument == null) { Debug.LogError("[MainMenu] UIDocument manquant !"); return; }
        var root = _uiDocument.rootVisualElement;
        if (root == null) { Debug.LogError("[MainMenu] rootVisualElement null."); return; }

        _root = root.Q<VisualElement>("root");
        _starsLayer = root.Q<VisualElement>("stars-layer");
        _menuContainer = root.Q<VisualElement>("menu-container"); // le bloc entier titre+boutons
        _btnStart = root.Q<Button>("btn-start");
        _btnOptions = root.Q<Button>("btn-options");

        _subMenu = BuildSubMenu();

        if (pixelFont != null) ApplyFontToAll(root, pixelFont);

        // Init Options avec le root et menuContainer communs
        optionsPanel?.Init(root, _menuContainer);

        _btnStart?.RegisterCallback<ClickEvent>(_ => { if (!_isAnimating) StartCoroutine(SlideToSubMenu()); });
        _btnOptions?.RegisterCallback<ClickEvent>(_ => optionsPanel?.Show());

        GenerateStars();
        StartCoroutine(AnimateStars());
        StartCoroutine(FadeIn());
    }

    // ── Sous-menu ─────────────────────────────────────────────────────────────

    private VisualElement BuildSubMenu()
    {
        var sub = new VisualElement();
        sub.name = "submenu-jouer";
        sub.style.position = Position.Absolute;
        sub.style.top = 0; sub.style.bottom = 0;
        sub.style.left = new Length(100, LengthUnit.Percent);
        sub.style.width = new Length(100, LengthUnit.Percent);
        sub.style.alignItems = Align.Center;
        sub.style.justifyContent = Justify.Center;
        sub.style.flexDirection = FlexDirection.Column;

        var titre = new Label("JOUER");
        titre.style.fontSize = 20;
        titre.style.color = new Color(0.83f, 0.66f, 1f);
        titre.style.marginBottom = 24;
        titre.style.unityTextAlign = TextAnchor.MiddleCenter;
        sub.Add(titre);

        var frame = new VisualElement();
        frame.style.flexDirection = FlexDirection.Column;
        frame.style.alignItems = Align.Center;
        frame.style.paddingTop = 28; frame.style.paddingBottom = 28;
        frame.style.paddingLeft = 48; frame.style.paddingRight = 48;
        frame.style.borderTopWidth = 2; frame.style.borderBottomWidth = 2;
        frame.style.borderLeftWidth = 2; frame.style.borderRightWidth = 2;
        frame.style.borderTopColor = new Color(0.23f, 0.10f, 0.43f);
        frame.style.borderBottomColor = new Color(0.23f, 0.10f, 0.43f);
        frame.style.borderLeftColor = new Color(0.23f, 0.10f, 0.43f);
        frame.style.borderRightColor = new Color(0.23f, 0.10f, 0.43f);
        frame.style.backgroundColor = new Color(0.08f, 0.03f, 0.16f, 0.6f);

        var btnHistoire = MakeSubButton("HISTOIRE", new Color(0.5f, 0.25f, 0.8f));
        btnHistoire.RegisterCallback<ClickEvent>(_ => { if (!_isAnimating) StartCoroutine(TransitionTo(histoireSceneName)); });
        frame.Add(btnHistoire);

        var divider = new VisualElement();
        divider.style.width = 240;
        divider.style.height = 2;
        divider.style.backgroundColor = new Color(0.23f, 0.10f, 0.43f);
        divider.style.marginTop = 14; divider.style.marginBottom = 14;
        frame.Add(divider);

        var btnArena = MakeSubButton("ARENA", new Color(0.8f, 0.15f, 0.15f));
        btnArena.RegisterCallback<ClickEvent>(_ => { if (!_isAnimating) StartCoroutine(TransitionTo(arenaSceneName)); });
        frame.Add(btnArena);

        sub.Add(frame);

        var btnRetour = MakeSubButton("< RETOUR", new Color(0.3f, 0.3f, 0.45f));
        btnRetour.style.marginTop = 20;
        btnRetour.RegisterCallback<ClickEvent>(_ => { if (!_isAnimating) StartCoroutine(SlideBackToMain()); });
        sub.Add(btnRetour);

        _root.Add(sub);
        return sub;
    }

    private Button MakeSubButton(string label, Color borderColor)
    {
        var btn = new Button();
        btn.text = label;
        btn.style.width = 240;
        btn.style.height = 48;
        btn.style.fontSize = 13;
        btn.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
        btn.style.borderTopColor = borderColor;
        btn.style.borderBottomColor = borderColor;
        btn.style.borderLeftColor = borderColor;
        btn.style.borderRightColor = borderColor;
        btn.style.borderTopWidth = 2; btn.style.borderBottomWidth = 2;
        btn.style.borderLeftWidth = 2; btn.style.borderRightWidth = 2;
        btn.style.color = new Color(0.88f, 0.78f, 1f);
        btn.style.unityTextAlign = TextAnchor.MiddleCenter;
        return btn;
    }

    // ── Slide animations ──────────────────────────────────────────────────────

    private IEnumerator SlideToSubMenu()
    {
        _isAnimating = true;
        _btnStart?.SetEnabled(false);
        _btnOptions?.SetEnabled(false);

        // Position de départ
        float menuStartX = _menuContainer.resolvedStyle.left;
        float subStartX = _root.resolvedStyle.width; // 100% width = hors écran droite

        float menuEndX = -subStartX;  // sort par la gauche
        float subEndX = 0f;          // entre par la droite

        float duration = 0.35f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);

            _menuContainer.style.translate = new Translate(Mathf.Lerp(0f, -subStartX, p), 0f);
            _subMenu.style.left = new Length(Mathf.Lerp(100f, 0f, p), LengthUnit.Percent);
            yield return null;
        }

        _menuContainer.style.translate = new Translate(-subStartX, 0f);
        _subMenu.style.left = Length.Auto();
        _subMenu.style.left = new Length(0, LengthUnit.Percent);

        _isAnimating = false;
    }

    private IEnumerator SlideBackToMain()
    {
        _isAnimating = true;

        float width = _root.resolvedStyle.width;
        float duration = 0.35f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);

            _menuContainer.style.translate = new Translate(Mathf.Lerp(-width, 0f, p), 0f);
            _subMenu.style.left = new Length(Mathf.Lerp(0f, 100f, p), LengthUnit.Percent);
            yield return null;
        }

        _menuContainer.style.translate = new Translate(0f, 0f);
        _subMenu.style.left = new Length(100, LengthUnit.Percent);

        _btnStart?.SetEnabled(true);
        _btnOptions?.SetEnabled(true);
        _isAnimating = false;
    }

    // ── Transition scène ──────────────────────────────────────────────────────

    private IEnumerator TransitionTo(string sceneName)
    {
        _isAnimating = true;
        if (_audio != null) StartCoroutine(_audio.FadeOutAndStop());

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            if (_root != null) _root.style.opacity = 1f - Mathf.Clamp01(elapsed / transitionDuration);
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }

    // ── Fade In ───────────────────────────────────────────────────────────────

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        const float duration = 0.8f;
        if (_root != null) _root.style.opacity = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (_root != null) _root.style.opacity = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        if (_root != null) _root.style.opacity = 1f;
    }

    // ── Police ────────────────────────────────────────────────────────────────

    private void ApplyFontToAll(VisualElement element, Font font)
    {
        var fontDef = FontDefinition.FromFont(font);
        if (element is Label l) l.style.unityFontDefinition = new StyleFontDefinition(fontDef);
        if (element is Button b) b.style.unityFontDefinition = new StyleFontDefinition(fontDef);
        foreach (var child in element.Children()) ApplyFontToAll(child, font);
    }

    // ── Étoiles ───────────────────────────────────────────────────────────────

    private void GenerateStars()
    {
        if (_starsLayer == null) return;
        _stars = new VisualElement[_starCount];
        for (int i = 0; i < _starCount; i++)
        {
            var star = new VisualElement();
            star.style.position = Position.Absolute;
            star.style.width = Random.Range(1, 3);
            star.style.height = star.style.width;
            star.style.backgroundColor = new Color(1f, 1f, 1f, 0f);
            star.style.left = new Length(Random.Range(0f, 100f), LengthUnit.Percent);
            star.style.top = new Length(Random.Range(0f, 40f), LengthUnit.Percent);
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
            phases[i] = Random.Range(0f, Mathf.PI * 2f);
            speeds[i] = Random.Range(0.5f, 2f);
            maxAlphas[i] = Random.Range(0.3f, 0.9f);
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
}