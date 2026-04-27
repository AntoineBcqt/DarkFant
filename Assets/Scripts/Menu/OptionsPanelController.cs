using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Panneau Options intégré dans le même UIDocument que le MainMenu.
/// S'affiche via slide depuis la droite — Init() appelé par MainMenuController.
/// </summary>
public class OptionsPanelController : MonoBehaviour
{
    [Header("Font")]
    [SerializeField] private Font pixelFont;

    private VisualElement _root;       // root du UIDocument principal
    private VisualElement _panel;      // le panneau options construit par code
    private VisualElement _menuContainer; // le menu principal à slider

    private Slider _sliderMusic;
    private Slider _sliderSfx;
    private Label _labelMusicVal;
    private Label _labelSfxVal;
    private Toggle _toggleFullscreen;
    private DropdownField _dropdownResolution;

    private bool _isAnimating = false;

    private readonly List<string> _resolutions = new()
    {
        "1920 x 1080", "1600 x 900", "1366 x 768", "1280 x 720"
    };

    /// <summary>Appelé par MainMenuController.Start() avec le root commun.</summary>
    public void Init(VisualElement root, VisualElement menuContainer)
    {
        _root = root;
        _menuContainer = menuContainer;
        _panel = BuildPanel();
        LoadSavedPrefs();
        if (pixelFont != null) ApplyFontToAll(_panel, pixelFont);
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    private VisualElement BuildPanel()
    {
        var panel = new VisualElement();
        panel.name = "options-panel";
        panel.style.position = Position.Absolute;
        panel.style.top = 0; panel.style.bottom = 0;
        panel.style.left = new Length(100, LengthUnit.Percent);
        panel.style.width = new Length(100, LengthUnit.Percent);
        panel.style.alignItems = Align.Center;
        panel.style.justifyContent = Justify.Center;
        panel.style.flexDirection = FlexDirection.Column;

        // Titre
        var titre = new Label("OPTIONS");
        titre.style.fontSize = 20;
        titre.style.color = new Color(0.83f, 0.66f, 1f);
        titre.style.marginBottom = 24;
        titre.style.unityTextAlign = TextAnchor.MiddleCenter;
        panel.Add(titre);

        // Cadre
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
        frame.style.width = 320;

        // Musique
        frame.Add(MakeLabel("MUSIQUE"));
        _sliderMusic = new Slider(0f, 1f) { value = 0.7f };
        _sliderMusic.style.width = 260;
        _labelMusicVal = new Label("70");
        _labelMusicVal.style.color = new Color(0.83f, 0.66f, 1f);
        _labelMusicVal.style.fontSize = 11;
        _labelMusicVal.style.unityTextAlign = TextAnchor.MiddleCenter;
        _sliderMusic.RegisterValueChangedCallback(e =>
            _labelMusicVal.text = Mathf.RoundToInt(e.newValue * 100).ToString());
        frame.Add(_sliderMusic);
        frame.Add(_labelMusicVal);

        frame.Add(MakeDivider());

        // SFX
        frame.Add(MakeLabel("EFFETS SONORES"));
        _sliderSfx = new Slider(0f, 1f) { value = 1f };
        _sliderSfx.style.width = 260;
        _labelSfxVal = new Label("100");
        _labelSfxVal.style.color = new Color(0.83f, 0.66f, 1f);
        _labelSfxVal.style.fontSize = 11;
        _labelSfxVal.style.unityTextAlign = TextAnchor.MiddleCenter;
        _sliderSfx.RegisterValueChangedCallback(e =>
            _labelSfxVal.text = Mathf.RoundToInt(e.newValue * 100).ToString());
        frame.Add(_sliderSfx);
        frame.Add(_labelSfxVal);

        frame.Add(MakeDivider());

        // Plein écran
        frame.Add(MakeLabel("PLEIN ÉCRAN"));
        _toggleFullscreen = new Toggle { value = Screen.fullScreen };
        _toggleFullscreen.style.marginBottom = 8;
        frame.Add(_toggleFullscreen);

        frame.Add(MakeDivider());

        // Résolution
        frame.Add(MakeLabel("RÉSOLUTION"));
        _dropdownResolution = new DropdownField(_resolutions, 0);
        _dropdownResolution.style.width = 260;
        _dropdownResolution.style.marginBottom = 8;
        frame.Add(_dropdownResolution);

        // Bouton APPLIQUER
        var btnApply = MakeButton("APPLIQUER", new Color(0.5f, 0.25f, 0.8f));
        btnApply.style.marginTop = 16;
        btnApply.RegisterCallback<ClickEvent>(_ => ApplySettings());
        frame.Add(btnApply);

        panel.Add(frame);

        // Bouton RETOUR
        var btnRetour = MakeButton("< RETOUR", new Color(0.3f, 0.3f, 0.45f));
        btnRetour.style.marginTop = 20;
        btnRetour.RegisterCallback<ClickEvent>(_ => { if (!_isAnimating) StartCoroutine(SlideBack()); });
        panel.Add(btnRetour);

        panel.style.display = DisplayStyle.None;
        _root.Add(panel);
        return panel;
    }

    private Label MakeLabel(string text)
    {
        var lbl = new Label(text);
        lbl.style.color = new Color(0.65f, 0.5f, 0.85f);
        lbl.style.fontSize = 10;
        lbl.style.marginBottom = 6;
        lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
        return lbl;
    }

    private VisualElement MakeDivider()
    {
        var d = new VisualElement();
        d.style.width = 260;
        d.style.height = 1;
        d.style.backgroundColor = new Color(0.23f, 0.10f, 0.43f);
        d.style.marginTop = 12; d.style.marginBottom = 12;
        return d;
    }

    private Button MakeButton(string label, Color borderColor)
    {
        var btn = new Button();
        btn.text = label;
        btn.style.width = 240;
        btn.style.height = 44;
        btn.style.fontSize = 12;
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

    // ── Slide ─────────────────────────────────────────────────────────────────

    public void Show()
    {
        Debug.Log($"[Options] Show() appelé — _panel null: {_panel == null}, _root null: {_root == null}");
        if (!_isAnimating) StartCoroutine(SlideIn());
    }

    private IEnumerator SlideIn()
    {
        _isAnimating = true;
        _panel.style.display = DisplayStyle.Flex;
        _panel.style.left = new Length(100, LengthUnit.Percent);
        float width = _root.resolvedStyle.width;
        float duration = 0.35f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);
            _menuContainer.style.translate = new Translate(Mathf.Lerp(0f, -width, p), 0f);
            _panel.style.left = new Length(Mathf.Lerp(100f, 0f, p), LengthUnit.Percent);
            yield return null;
        }

        _menuContainer.style.translate = new Translate(-width, 0f);
        _panel.style.left = new Length(0, LengthUnit.Percent);
        _isAnimating = false;
    }

    private IEnumerator SlideBack()
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
            _panel.style.left = new Length(Mathf.Lerp(0f, 100f, p), LengthUnit.Percent);
            yield return null;
        }

        _menuContainer.style.translate = new Translate(0f, 0f);
        _panel.style.left = new Length(100, LengthUnit.Percent);
        _panel.style.display = DisplayStyle.None;
        _isAnimating = false;
    }

    // ── Appliquer ─────────────────────────────────────────────────────────────

    private void ApplySettings()
    {
        if (_sliderMusic != null) AudioListener.volume = _sliderMusic.value;
        if (_toggleFullscreen != null) Screen.fullScreen = _toggleFullscreen.value;

        if (_dropdownResolution != null)
        {
            var parts = _dropdownResolution.value.Replace(" ", "").Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                Screen.SetResolution(w, h, Screen.fullScreen);
        }

        PlayerPrefs.SetFloat("MusicVolume", _sliderMusic?.value ?? 0.7f);
        PlayerPrefs.SetFloat("SFXVolume", _sliderSfx?.value ?? 1f);
        PlayerPrefs.SetInt("Fullscreen", (_toggleFullscreen?.value ?? true) ? 1 : 0);
        PlayerPrefs.Save();

        StartCoroutine(SlideBack());
    }

    // ── Prefs ─────────────────────────────────────────────────────────────────

    private void LoadSavedPrefs()
    {
        if (_sliderMusic != null)
        {
            _sliderMusic.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            if (_labelMusicVal != null)
                _labelMusicVal.text = Mathf.RoundToInt(_sliderMusic.value * 100).ToString();
        }
        if (_sliderSfx != null)
        {
            _sliderSfx.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            if (_labelSfxVal != null)
                _labelSfxVal.text = Mathf.RoundToInt(_sliderSfx.value * 100).ToString();
        }
        if (_toggleFullscreen != null)
            _toggleFullscreen.value = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }

    // ── Police ────────────────────────────────────────────────────────────────

    private void ApplyFontToAll(VisualElement element, Font font)
    {
        var fontDef = FontDefinition.FromFont(font);
        if (element is Label l) l.style.unityFontDefinition = new StyleFontDefinition(fontDef);
        if (element is Button b) b.style.unityFontDefinition = new StyleFontDefinition(fontDef);
        foreach (var child in element.Children()) ApplyFontToAll(child, font);
    }
}