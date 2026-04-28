using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;
using UnityEngine.InputSystem; // <-- 1. BIEN VÉRIFIER QUE CETTE LIGNE EST LÀ

public class MentorNPC : MonoBehaviour
{
    [Header("UI & Texte")]
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public string[] messages;

    [Header("Paramètres du Twist")]
    public CinemachineCamera vcam;
    public Light2D globalLight;
    public float shakeIntensity = 3f;
    public float twistDuration = 2.5f;

    private int index = 0;
    private bool playerInRange = false;

    private Color initialColor;
    private float initialIntensity;

    void Start()
    {
        if (globalLight != null)
        {
            initialColor = globalLight.color;
            initialIntensity = globalLight.intensity;
        }
    }

    // --- LE BLOC À AJOUTER EST ICI ---
    void Update()
    {
        // On vérifie si le joueur est proche ET s'il vient d'appuyer sur E
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }
    // --------------------------------

    public void Interact()
    {
        if (!playerInRange) return;

        if (!dialogueUI.activeSelf)
        {
            index = 0;
            dialogueUI.SetActive(true);
            dialogueText.text = messages[index];
        }
        else
        {
            index++;
            if (index < messages.Length)
            {
                dialogueText.text = messages[index];
            }
            else
            {
                TriggerTwist();
            }
        }
    }

    void TriggerTwist()
    {
        dialogueUI.SetActive(false);

        // Au lieu de détruire le mentor, on cache son visuel pour que la Coroutine/Invoke puisse finir
        if (GetComponent<SpriteRenderer>()) GetComponent<SpriteRenderer>().enabled = false;
        if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = false;

        if (globalLight != null)
        {
            globalLight.color = Color.red;
            globalLight.intensity = 0.5f;
        }

        if (vcam != null)
        {
            var noise = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null)
            {
                noise.AmplitudeGain = shakeIntensity;
                noise.FrequencyGain = 1.5f;
            }
        }

        Invoke("ResetScene", twistDuration);
    }

    void ResetScene()
    {
        if (vcam != null)
        {
            var noise = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null) noise.AmplitudeGain = 0;
        }

        if (globalLight != null)
        {
            globalLight.color = initialColor;
            globalLight.intensity = initialIntensity;
        }

        Debug.Log("Reset terminé. Le mentor est parti, la lumière est revenue.");

        // On détruit l'objet à la toute fin
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueUI.SetActive(false);
        }
    }
}