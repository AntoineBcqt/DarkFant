using UnityEngine;
using UnityEngine.InputSystem; // Indispensable pour Unity 6

public class PorteVerrouillee : MonoBehaviour
{
    public GameObject messageE; // Le texte "Appuyez sur E" au-dessus de la porte
    private bool joueurProche = false;
    private PlayerInventory inventaire;

    void Start()
    {
        if (messageE != null) messageE.SetActive(false);
    }

    void Update()
    {
        // Si on est proche et qu'on appuie sur E
        if (joueurProche && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TenterDouvrir();
        }
    }

    void TenterDouvrir()
    {
        if (inventaire != null && inventaire.aLaClef)
        {
            Debug.Log("Sésame, ouvre-toi !");
            // On désactive toute la porte
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("La porte est verrouillée... Il me faut une clé !");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            joueurProche = true;
            inventaire = other.GetComponent<PlayerInventory>();
            if (messageE != null) messageE.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            joueurProche = false;
            if (messageE != null) messageE.SetActive(false);
        }
    }
}