using UnityEngine;
using TMPro; // Nécessaire pour TextMeshPro

public class InteractibleNote : MonoBehaviour
{
    public GameObject uiPanel;      // Glisse ton DialoguePanel ici
    public string message;          // Écris ton texte ici dans l'Inspector
    private bool isPlayerNearby;

    void Update()
    {
        // Si le joueur est proche et appuie sur E (ou Espace)
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleNote();
        }
    }

    void ToggleNote()
    {
        bool isActive = uiPanel.activeSelf;
        uiPanel.SetActive(!isActive); // Alterne entre affiché et caché
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // Optionnel : Afficher un petit message "Appuyez sur E pour lire"
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            uiPanel.SetActive(false); // Ferme la note si le joueur s'éloigne
        }
    }
}