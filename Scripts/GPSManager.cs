using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GPSManager : MonoBehaviour
{
    public float distanceTravelled = 0.0f;
    private Vector2 lastPosition;
    private bool isFirstPosition = true;

    // Referencia al texto del UI
    public Text distanceText;

    void Start()
    {
        StartCoroutine(StartLocationService());
    }

    public IEnumerator StartLocationService()  // Cambiado a public
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location service not enabled by user");
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }
        else
        {
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude);
            lastPosition = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
            isFirstPosition = false;
        }
    }

    void Update()
    {
        if (Input.location.status == LocationServiceStatus.Running && !isFirstPosition)
        {
            Vector2 currentPosition = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
            distanceTravelled += Vector2.Distance(lastPosition, currentPosition);
            lastPosition = currentPosition;

            // Actualizar el texto del UI
            distanceText.text = "Distance: " + (distanceTravelled / 1000).ToString("F2") + " km";

            if (distanceTravelled >= 5000)
            {
                GrantPoint();
                distanceTravelled = 0.0f;
            }
        }
    }

    void GrantPoint()
    {
        Debug.Log("You've earned 1 point!");
        // Implementa la lógica para añadir puntos a la puntuación del usuario
    }
}
