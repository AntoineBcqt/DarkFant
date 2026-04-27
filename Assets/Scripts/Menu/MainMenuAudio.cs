using System.Collections;
using UnityEngine;

/// <summary>
/// Gestion audio du menu principal - DarkFant
/// Attach sur le même GameObject que MainMenuController.
/// Assigner le clip "Mossgate Cavern" dans l'Inspector.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MainMenuAudio : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float targetVolume = 0.7f;

    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = menuMusic;
        _audioSource.loop = true;
        _audioSource.volume = 0f;
        _audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (menuMusic == null)
        {
            Debug.LogWarning("[MainMenuAudio] Aucun clip assigné — glisse Mossgate Cavern dans l'Inspector.");
            return;
        }

        _audioSource.Play();
        _fadeCoroutine = StartCoroutine(FadeVolume(0f, targetVolume, fadeInDuration));
    }

    /// <summary>
    /// Appelé par MainMenuController avant de charger la scène de jeu.
    /// </summary>
    public IEnumerator FadeOutAndStop()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        yield return StartCoroutine(FadeVolume(_audioSource.volume, 0f, fadeOutDuration));
        _audioSource.Stop();
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        _audioSource.volume = to;
    }
}