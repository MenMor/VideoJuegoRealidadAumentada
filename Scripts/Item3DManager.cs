using UnityEngine;

public class Item3DManager : MonoBehaviour
{
    private ARInteractionsManager aRInteractionsManager;

    void Start()
    {
        // Obtener la referencia al ARInteractionsManager
        aRInteractionsManager = FindObjectOfType<ARInteractionsManager>();
    }

    // Método llamado cuando el objeto entra en un trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BasuraElectronicos"))
        {
            // Usar el TextManager para mostrar el texto correcto
            TextManager.instance.ShowCorrectText();
            Destroy(gameObject);

            // Desactivar aRPointer
            if (aRInteractionsManager != null)
            {
                aRInteractionsManager.DeactivateARPointer();
            }
        }
        else if (other.gameObject.CompareTag("BasuraCarton") || other.gameObject.CompareTag("BasuraLata") || other.gameObject.CompareTag("BasuraPlastico"))
        {
            // Usar el TextManager para mostrar el texto incorrecto
            TextManager.instance.ShowIncorrectText();
        }
    }
}