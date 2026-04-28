using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Transition vers la BattleScene depuis la SampleScene.
/// Coco appelle TriggerBattleScene() depuis MentorNPC.TriggerTwist()
/// </summary>
public class BattleTransition : MonoBehaviour
{
    [Header("Scène")]
    public string battleSceneName = "BattleScene";

    [Header("Fondu")]
    public float fadeDuration = 1.5f;
    public Color fadeColor    = Color.black;

    private static BattleTransition _instance;
    public static BattleTransition Instance => _instance;

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
    }

    /// <summary>
    /// Méthode principale — Coco l'appelle dans MentorNPC.TriggerTwist()
    /// </summary>
    public void TriggerBattleScene()
    {
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        // Créer un overlay de fondu
        var canvasGO = new GameObject("_FadeCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        var img = imgGO.AddComponent<Image>();
        var rt  = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        img.color    = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        DontDestroyOnLoad(canvasGO);

        // Fondu vers noir
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            img.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }

        // Charger la BattleScene
        SceneManager.LoadScene(battleSceneName);

        // Fondu depuis le noir
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            img.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f - Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }

        Destroy(canvasGO);
    }
}
