using UnityEngine;
using TMPro;

public class InteractibleNote : MonoBehaviour
{
    public GameObject uiPanel;          // Le parchemin de dialogue
    public GameObject interactionPrompt; // Le petit message "[E] Lire"
    public TextMeshProUGUI noteText;    // Le composant texte DU parchemin
    [TextArea(3, 10)]
    public string message;              // Le contenu de ta note

    private bool isPlayerNearby;

    void Start()
    {
        // On s'assure que tout est caché au début
        if (uiPanel) uiPanel.SetActive(false);
        if (interactionPrompt) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleNote();
        }
    }

    void ToggleNote()
    {
        bool isActive = !uiPanel.activeSelf;
        uiPanel.SetActive(isActive);

        if (isActive)
        {
            noteText.text = message; // On injecte ton texte dans le parchemin
            interactionPrompt.SetActive(false); // On cache le [E] quand on lit
        }
        else
        {
            interactionPrompt.SetActive(true); // On remet le [E] quand on ferme
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (!uiPanel.activeSelf) interactionPrompt.SetActive(true); // Affiche [E]
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            interactionPrompt.SetActive(false); // Cache le [E]
            uiPanel.SetActive(false);           // Ferme la note
        }
    }
}