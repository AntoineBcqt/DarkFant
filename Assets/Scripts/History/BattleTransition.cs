using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleTransition : MonoBehaviour
{
    [Header("Scène")]
    public string battleSceneName = "BattleScene";

    [Header("Fondu")]
    public float fadeDuration = 1.0f;
    public Color fadeColor = Color.black;

    private static BattleTransition _instance;
    public static BattleTransition Instance => _instance;

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerBattleScene()
    {
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        // Créer overlay
        var canvasGO = new GameObject("_FadeCanvas");
        DontDestroyOnLoad(canvasGO);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        var img = imgGO.AddComponent<Image>();
        var rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        img.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        // Fade to black
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            img.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        img.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        // Charger la scène et attendre
        var op = SceneManager.LoadSceneAsync(battleSceneName);
        while (!op.isDone) yield return null;

        // Attendre un frame supplémentaire que la scène s'initialise
        yield return new WaitForSeconds(0.3f);

        // Fade from black
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            img.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f - Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }

        Destroy(canvasGO);
        Destroy(gameObject);
    }
}