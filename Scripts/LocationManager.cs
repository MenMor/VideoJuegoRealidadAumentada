using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }
    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(GetLocation());
    }

    public IEnumerator GetLocation()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location services are disabled.");
            yield break;
        }

        Input.location.Start(5f, 5f); // Start location services with desired accuracy and distance filter.

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location.");
            yield break;
        }
        else
        {
            Latitude = Input.location.lastData.latitude;
            Longitude = Input.location.lastData.longitude;
            Debug.Log($"Location: {Latitude}, {Longitude}");
        }

        Input.location.Stop();
    }
}
