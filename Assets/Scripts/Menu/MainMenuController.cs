using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private float transitionDuration = 0.6f;

    [Header("Font")]
    [SerializeField] private Font pixelFont;

    [Header("References")]
    [SerializeField] private OptionsPanelController optionsPanel;

    private UIDocument _uiDocument;
    private MainMenuAudio _audio;
    private Button _btnStart;
    private Button _btnOptions;
    private VisualElement _root;
    private VisualElement _starsLayer;

    private readonly int _starCount = 30;
    private VisualElement[] _stars;

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
        _btnStart = root.Q<Button>("btn-start");
        _btnOptions = root.Q<Button>("btn-options");

        if (pixelFont != null) ApplyFontToAll(root, pixelFont);

        _btnStart?.RegisterCallback<ClickEvent>(_ => OnStartClicked());
        _btnOptions?.RegisterCallback<ClickEvent>(_ => OnOptionsClicked());

        GenerateStars();
        StartCoroutine(AnimateStars());
        StartCoroutine(FadeIn());
    }

    private void ApplyFontToAll(VisualElement element, Font font)
    {
        var fontDef = FontDefinition.FromFont(font);
        if (element is Label l) l.style.unityFontDefinition = new StyleFontDefinition(fontDef);
        if (element is Button b) b.style.unityFontDefinition = new StyleFontDefinition(fontDef);
        foreach (var child in element.Children()) ApplyFontToAll(child, font);
    }

    private void OnStartClicked() => StartCoroutine(TransitionToGame());
    private void OnOptionsClicked()
    {
        Debug.Log($"[MainMenu] OPTIONS cliqué — optionsPanel null: {optionsPanel == null}");
        optionsPanel?.Show();
    }

    private IEnumerator TransitionToGame()
    {
        _btnStart?.SetEnabled(false);
        _btnOptions?.SetEnabled(false);

        if (_audio != null) StartCoroutine(_audio.FadeOutAndStop());

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            if (_root != null) _root.style.opacity = 1f - Mathf.Clamp01(elapsed / transitionDuration);
            yield return null;
        }
        SceneManager.LoadScene(gameSceneName);
    }

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