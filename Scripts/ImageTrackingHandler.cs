using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.LegacyInputHelpers;

public class ImageTrackingHandler : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager aRTrackedImageManager;
    [SerializeField] private GameObject[] aRModelsToPlace;
    [SerializeField] private GameObject[] infoPanelPrefabs;
    [SerializeField] private APIClient apiClient;

    private Dictionary<string, GameObject> aRModels = new Dictionary<string, GameObject>();
    private Dictionary<string, bool> modelState = new Dictionary<string, bool>();
    private Dictionary<string, GameObject> infoPanels = new Dictionary<string, GameObject>();

    private string categoryImage;

    [SerializeField] private UIManager uiManager;

    void Start()
    {
        foreach (var aRModel in aRModelsToPlace)
        {
            GameObject newARModel = Instantiate(aRModel, Vector3.zero, Quaternion.identity);
            newARModel.name = aRModel.name;
            aRModels.Add(newARModel.name, newARModel);
            newARModel.SetActive(false);
            modelState.Add(newARModel.name, false);
        }
        foreach (var panelPrefab in infoPanelPrefabs)
        {
            GameObject newPanel = Instantiate(panelPrefab);
            newPanel.name = panelPrefab.name;
            infoPanels.Add(newPanel.name, newPanel);
            newPanel.SetActive(false);
        }

        if (apiClient == null)
        {
            apiClient = FindObjectOfType<APIClient>();
        }

        uiManager = FindObjectOfType<UIManager>();

        categoryImage = aRModelsToPlace[0].name;
        APIClient.ImageCategory = categoryImage;

        // Start LocationManager to get the GPS coordinates
        StartCoroutine(InitializeLocation());
    }

    private IEnumerator InitializeLocation()
    {
        yield return LocationManager.Instance.GetLocation();
    }

    void OnEnable()
    {
        aRTrackedImageManager.trackedImagesChanged += ImageFound;
    }



    void OnDisable()
    {
        aRTrackedImageManager.trackedImagesChanged -= ImageFound;
    }

    private void ImageFound(ARTrackedImagesChangedEventArgs eventData)
    {
        foreach (var trackedImage in eventData.added)
        {
            ShowARModel(trackedImage);
        }

        foreach (var trackedImage in eventData.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                ShowARModel(trackedImage);
            }
            else if (trackedImage.trackingState == TrackingState.Limited)
            {
                HideARModel(trackedImage);
            }
        }

        foreach (var trackedImage in eventData.removed)
        {
            HideARModel(trackedImage);
        }
    }

    private void ShowARModel(ARTrackedImage trackedImage)
    {
        bool isModelActivated = modelState[trackedImage.referenceImage.name];

        if (!isModelActivated)
        {
            GameObject aRModel = aRModels[trackedImage.referenceImage.name];
            aRModel.transform.position = trackedImage.transform.position;
            aRModel.SetActive(true);
            modelState[trackedImage.referenceImage.name] = true;

            categoryImage = trackedImage.referenceImage.name; 
            APIClient.ImageCategory = categoryImage;

            // Activar el menú de ítems para imágenes específicas
            if (trackedImage.referenceImage.name == "Plastico")
            {
                uiManager.ActivateItemsMenu("Plástico");
            }
            else if (trackedImage.referenceImage.name == "Paper")
            {
                uiManager.ActivateItemsMenu("Cartón/Papel");
            }

            // Save the user's GPS coordinates
            if (apiClient != null && LocationManager.Instance != null)
            {
                float latitude = LocationManager.Instance.Latitude;
                float longitude = LocationManager.Instance.Longitude;
                string category = trackedImage.referenceImage.name;
                APIClient.ImageCategory = category;

                if (latitude != 0 && longitude != 0)
                {
                    apiClient.SaveUserPosition(latitude, longitude, category);
                }
                else
                {
                    Debug.LogWarning("GPS coordinates are not available yet.");
                }

            }
        }
        else
        {
            GameObject aRModel = aRModels[trackedImage.referenceImage.name];
            aRModel.transform.position = trackedImage.transform.position;
        }
        if (infoPanels.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject infoPanel = infoPanels[trackedImage.referenceImage.name];
            infoPanel.SetActive(true);
        }
    }

    private void HideARModel(ARTrackedImage trackedImage)
    {
        bool isModelActivated = modelState[trackedImage.referenceImage.name];
        if (isModelActivated)
        {
            GameObject aRModel = aRModels[trackedImage.referenceImage.name];
            aRModel.SetActive(false);
            modelState[trackedImage.referenceImage.name] = false;
        }
        if (infoPanels.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject infoPanel = infoPanels[trackedImage.referenceImage.name];
            infoPanel.SetActive(false);
        }
    }

}