using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Contrôleur du panneau Options - DarkFant
/// Nécessite un GameObject séparé avec son propre UIDocument (Sort Order: 1).
/// Le UXML OptionsPanel.uxml doit être assigné dans ce UIDocument.
/// </summary>
public class OptionsPanelController : MonoBehaviour
{
    [Header("Font - même .ttf que MainMenuController")]
    [SerializeField] private Font pixelFont;

    private UIDocument _uiDocument;
    private Slider _sliderMusic;
    private Slider _sliderSfx;
    private Label _labelMusicVal;
    private Label _labelSfxVal;
    private Toggle _toggleFullscreen;
    private DropdownField _dropdownResolution;
    private Button _btnApply;
    private Button _btnClose;

    private readonly List<string> _resolutions = new()
    {
        "1920 x 1080",
        "1600 x 900",
        "1366 x 768",
        "1280 x 720",
    };

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        if (_uiDocument == null) { Debug.LogError("[Options] UIDocument manquant !"); return; }

        var root = _uiDocument.rootVisualElement;

        _sliderMusic = root.Q<Slider>("slider-music");
        _sliderSfx = root.Q<Slider>("slider-sfx");
        _labelMusicVal = root.Q<Label>("label-music-val");
        _labelSfxVal = root.Q<Label>("label-sfx-val");
        _toggleFullscreen = root.Q<Toggle>("toggle-fullscreen");
        _dropdownResolution = root.Q<DropdownField>("dropdown-resolution");
        _btnApply = root.Q<Button>("btn-apply");
        _btnClose = root.Q<Button>("btn-close");

        if (_dropdownResolution != null)
        {
            _dropdownResolution.choices = _resolutions;
            _dropdownResolution.value = _resolutions[0];
        }

        if (_toggleFullscreen != null)
            _toggleFullscreen.value = Screen.fullScreen;

        LoadSavedPrefs();

        _sliderMusic?.RegisterValueChangedCallback(e =>
        {
            if (_labelMusicVal != null)
                _labelMusicVal.text = Mathf.RoundToInt(e.newValue * 100).ToString();
        });

        _sliderSfx?.RegisterValueChangedCallback(e =>
        {
            if (_labelSfxVal != null)
                _labelSfxVal.text = Mathf.RoundToInt(e.newValue * 100).ToString();
        });

        _btnApply?.RegisterCallback<ClickEvent>(_ => ApplySettings());
        _btnClose?.RegisterCallback<ClickEvent>(_ => Hide());

        if (pixelFont != null) ApplyFontToAll(root, pixelFont);

        Hide();
    }

    // ─── Show / Hide ────────────────────────────────────────────────────────

    public void Show()
    {
        Debug.Log($"[Options] Show() — _uiDocument null: {_uiDocument == null}");
        if (_uiDocument != null)
        {
            Debug.Log($"[Options] rootVisualElement null: {_uiDocument.rootVisualElement == null}");
            _uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        }
    }

    public void Hide()
    {
        if (_uiDocument != null)
            _uiDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    // ─── Appliquer ──────────────────────────────────────────────────────────

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

        Hide();
    }

    // ─── Prefs ──────────────────────────────────────────────────────────────

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

    // ─── Police ─────────────────────────────────────────────────────────────

    private void ApplyFontToAll(VisualElement element, Font font)
    {
        var fontDef = FontDefinition.FromFont(font);
        if (element is Label l) l.style.unityFontDefinition = new StyleFontDefinition(fontDef);
        if (element is Button b) b.style.unityFontDefinition = new StyleFontDefinition(fontDef);
        foreach (var child in element.Children()) ApplyFontToAll(child, font);
    }
}