using UnityEngine;

public class CartonCollider : MonoBehaviour
{
    private ARInteractionsManager aRInteractionsManager;

    void Start()
    {
        // Obtener la referencia al ARInteractionsManager
        aRInteractionsManager = FindObjectOfType<ARInteractionsManager>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BasuraCarton"))
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
        else if (other.gameObject.CompareTag("BasuraPlastico") || other.gameObject.CompareTag("BasuraLata") || other.gameObject.CompareTag("BasuraElectronicos"))
        {
            // Usar el TextManager para mostrar el texto incorrecto
            TextManager.instance.ShowIncorrectText();
        }
    }
}
