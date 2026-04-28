using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Game Over screen style dark fantasy pixel art.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Header("Références UI")]
    public CanvasGroup screenGroup;
    public TextMeshProUGUI titleText;
    public Button retryButton;
    public Button quitButton;

    [Header("Scènes")]
    public string gameSceneName = "TempActionScene";
    public string menuSceneName = "MainMenuScene";

    [Header("Animation")]
    public float fadeInDuration = 1.2f;

    private void Awake()
    {
        // Ne pas désactiver ici — le builder le fait au moment de la création
        Debug.Log("[GameOver] Awake appelé");
    }

    public void ShowDelayed(float delay)
    {
        Debug.Log($"[GameOver] ShowDelayed appelé avec delay={delay}");
        var runner = new GameObject("_CoroutineRunner");
        var cr = runner.AddComponent<CoroutineRunner>();
        cr.Run(delay, this);
    }

    public void Show()
    {
        Debug.Log("[GameOver] Show() — activation du GO");
        gameObject.SetActive(true);
        if (screenGroup != null)
            screenGroup.alpha = 0f;
        else
            Debug.LogWarning("[GameOver] screenGroup est NULL !");
    }

    public IEnumerator FadeInPublic()
    {
        Debug.Log("[GameOver] FadeIn démarré");
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            if (screenGroup != null)
                screenGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }
        if (screenGroup != null) screenGroup.alpha = 1f;
        Debug.Log("[GameOver] FadeIn terminé — écran visible");
    }

    public void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}

/// <summary>Helper pour lancer une coroutine depuis un objet inactif.</summary>
public class CoroutineRunner : MonoBehaviour
{
    public void Run(float delay, GameOverScreen screen)
    {
        Debug.Log($"[CoroutineRunner] Démarrage wait {delay}s");
        StartCoroutine(Wait(delay, screen));
    }

    private IEnumerator Wait(float delay, GameOverScreen screen)
    {
        yield return new WaitForSecondsRealtime(delay);
        Debug.Log("[CoroutineRunner] Délai terminé — activation GameOver");
        screen.gameObject.SetActive(true);
        if (screen.screenGroup != null) screen.screenGroup.alpha = 0f;
        // Ne pas détruire avant la fin du FadeIn !
        yield return screen.FadeInPublic();
        Debug.Log("[CoroutineRunner] FadeIn terminé");
        Destroy(gameObject);
    }
}