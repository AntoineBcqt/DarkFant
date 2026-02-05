using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;

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

    private int index = 0;
    private bool playerInRange = false;

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

        this.gameObject.SetActive(false);

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

        Debug.Log("SÉISME ! Le Fléau arrive.");
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