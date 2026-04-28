using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // INDISPENSABLE pour l'attente

public class Coffre : MonoBehaviour
{
    public GameObject messageE;      // Le texte "Appuyez sur E" (World Space)
    public GameObject messageKeyUI; // Le texte "Clé récupérée !" (Overlay)
    public float tempsAffichage = 3f; // Temps en secondes

    private bool joueurProche = false;
    private PlayerInventory inventaire;

    void Update()
    {
        if (joueurProche && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OuvrirCoffre();
        }
    }

    void OuvrirCoffre()
    {
        inventaire.aLaClef = true;

        // On active le message de succès sur l'écran
        if (messageKeyUI != null)
        {
            messageKeyUI.SetActive(true);
            // On lance le compte à rebours pour le cacher
            StartCoroutine(CacherMessageApresDelai());
        }

        messageE.SetActive(false);
        // On ne détruit pas l'objet tout de suite pour laisser la Coroutine finir
        // On cache juste le visuel du coffre
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    IEnumerator CacherMessageApresDelai()
    {
        yield return new WaitForSeconds(tempsAffichage);
        messageKeyUI.SetActive(false);

        // Maintenant on peut supprimer le coffre si on veut
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            joueurProche = true;
            inventaire = other.GetComponent<PlayerInventory>();
            messageE.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            joueurProche = false;
            messageE.SetActive(false);
        }
    }
}