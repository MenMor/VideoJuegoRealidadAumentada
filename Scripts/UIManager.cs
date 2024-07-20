using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject itemsMenuCanvas;
    [SerializeField] private GameObject ARPositionCanvas;
    [SerializeField] private GameObject quizCanvas;

    [SerializeField] private List<GameObject> objectsToSpawn;
    private float minChangeInterval = 2.0f;
    private float maxChangeInterval = 10.0f;
    private static GameObject currentObject;
    private Camera arCamera;
    private float timeSinceLastChange = 0.0f;
    private float changeInterval;

    private List<GameObject> objectPool = new List<GameObject>();

    private ARInteractionsManager aRInteractionsManager;

    private DataManager dataManager;

    void Start()
    {
        GameManager.instance.OnMainMenu += ActivateMainMenu;
        GameManager.instance.OnItemsMenu += ActivateItemsMenu;
        GameManager.instance.OnARPosition += ActivateARPosition;
        GameManager.instance.OnQuiz += ActivateQuiz;

        mainMenuCanvas.SetActive(true);
        itemsMenuCanvas.SetActive(false);
        ARPositionCanvas.SetActive(false);
        quizCanvas.SetActive(false);

        arCamera = Camera.main;

        if (!quizCanvas.activeSelf)
        {
            CreateObjectPool();
            changeInterval = UnityEngine.Random.Range(minChangeInterval, maxChangeInterval);
        }

        aRInteractionsManager = FindObjectOfType<ARInteractionsManager>();
        dataManager = FindObjectOfType<DataManager>();
    }

    void Update()
    {

        if (ARPositionCanvas.activeSelf)
        {
            timeSinceLastChange += Time.deltaTime;
            if (timeSinceLastChange >= changeInterval)
            {
                ChangeObject();
                timeSinceLastChange = 0.0f;
            }
        }
    }


    private void CreateObjectPool()
    {
        foreach (GameObject prefab in objectsToSpawn)
        {
            GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            obj.SetActive(false);
            objectPool.Add(obj);

            // Añadir un TextMeshProUGUI para mostrar el nombre del objeto
            TextMeshProUGUI textMesh = obj.AddComponent<TextMeshProUGUI>();
            textMesh.text = prefab.name;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontSize = 5;
            textMesh.color = Color.white;
            textMesh.rectTransform.pivot = new Vector2(0.5f, 0);
            textMesh.transform.localPosition = new Vector3(0, 1f, 0);
        }
    }



    private void ActivateMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        mainMenuCanvas.transform.GetChild(0).transform.DOScale(new Vector3(1, 1, 1), 0.3f);
        mainMenuCanvas.transform.GetChild(1).transform.DOScale(new Vector3(1, 1, 1), 0.3f);
        mainMenuCanvas.transform.GetChild(2).transform.DOScale(new Vector3(1, 1, 1), 0.3f);

        foreach (GameObject obj in objectPool)
        {
            obj.SetActive(false);
        }

        itemsMenuCanvas.SetActive(false);
        ARPositionCanvas.SetActive(false);
        quizCanvas.SetActive(false);

        // Destruir item3DModel
        if (aRInteractionsManager != null)
        {
            aRInteractionsManager.DestroyItem3DModel();
        }
    }

    public void ActivateItemsMenu()
    {
        mainMenuCanvas.SetActive(false);
        itemsMenuCanvas.SetActive(true);
        itemsMenuCanvas.transform.GetChild(0).transform.DOScale(new Vector3(1, 1, 1), 0.5f);
        itemsMenuCanvas.transform.GetChild(1).transform.DOScale(new Vector3(1, 1, 1), 0.3f);
        itemsMenuCanvas.transform.GetChild(1).transform.DOMoveY(300, 0.3f);

        ARPositionCanvas.SetActive(false);
        quizCanvas.SetActive(false);

        // Destruir item3DModel
        if (aRInteractionsManager != null)
        {
            aRInteractionsManager.DestroyItem3DModel();
        }
    }
    public void ActivateItemsMenu(string itemNameToActivate)
    {
        mainMenuCanvas.SetActive(false);
        itemsMenuCanvas.SetActive(true);
        itemsMenuCanvas.transform.GetChild(0).transform.DOScale(new Vector3(1, 1, 1), 0.5f);
        itemsMenuCanvas.transform.GetChild(1).transform.DOScale(new Vector3(1, 1, 1), 0.3f);
        itemsMenuCanvas.transform.GetChild(1).transform.DOMoveY(300, 0.3f);

        ARPositionCanvas.SetActive(false);
        quizCanvas.SetActive(false);

        // Destruir item3DModel
        if (aRInteractionsManager != null)
        {
            aRInteractionsManager.DestroyItem3DModel();
        }

        // Crear y activar solo el botón específico
        dataManager.CreateSpecificButton(itemNameToActivate);
    }

    private void ActivateARPosition()
    {
        mainMenuCanvas.SetActive(false);
        itemsMenuCanvas.SetActive(false);
        ARPositionCanvas.SetActive(true);
        ARPositionCanvas.transform.GetChild(0).transform.DOScale(new Vector3(1, 1, 1), 0.3f);

        quizCanvas.SetActive(false);
    }

    private void ActivateQuiz()
    {
        mainMenuCanvas.SetActive(false);
        itemsMenuCanvas.SetActive(false);
        ARPositionCanvas.SetActive(false);

        foreach (GameObject obj in objectPool)
        {
            obj.SetActive(false);
        }

        quizCanvas.SetActive(true);
        quizCanvas.transform.GetChild(0).transform.DOScale(new Vector3(1, 1, 1), 0.3f);

        // Destruir item3DModel
        if (aRInteractionsManager != null)
        {
            aRInteractionsManager.DestroyItem3DModel();
        }

    }

    private void ChangeObject()
    {
        if (quizCanvas.activeSelf)
        {
            return;
        }

        // Deactivate the previous objects if they exist
        foreach (GameObject obj in objectPool)
        {
            obj.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
            obj.SetActive(false);
        }

        // Define positions for the objects in a row, closer together and centered around the yellow area
        float distanceFromCamera = 1.0f;
        Vector3 centerPosition = arCamera.transform.position + arCamera.transform.forward * distanceFromCamera + new Vector3(0, -0.3f, 0); // Adjusted vertically to the yellow area
        Vector3[] positions = new Vector3[4];
        float offset = 0.1f; // Value to set the distance between objects
        positions[0] = centerPosition + arCamera.transform.right * (-1.5f * offset); // Far left
        positions[1] = centerPosition + arCamera.transform.right * (-0.5f * offset); // Left
        positions[2] = centerPosition + arCamera.transform.right * (0.5f * offset);  // Right
        positions[3] = centerPosition + arCamera.transform.right * (1.5f * offset);  // Far right

        // Randomly choose four different objects from the pool
        List<int> indices = new List<int>();
        while (indices.Count < 4)
        {
            int index = UnityEngine.Random.Range(0, objectPool.Count);
            if (!indices.Contains(index))
            {
                indices.Add(index);
            }
        }

        // Activate and position the chosen objects
        for (int i = 0; i < 4; i++)
        {
            currentObject = objectPool[indices[i]];
            currentObject.transform.position = positions[i];
            currentObject.transform.localScale = Vector3.one * 0.15f; // Smaller scale
            currentObject.transform.rotation = Quaternion.Euler(0, arCamera.transform.eulerAngles.y + 180, 0);
            currentObject.SetActive(true);

            TextMeshProUGUI textMesh = currentObject.GetComponentInChildren<TextMeshProUGUI>();
            textMesh.gameObject.SetActive(true);
            textMesh.transform.position = positions[i] + new Vector3(0, 0.2f, 0); // Position title above object
        }

        // Calculate a new change interval
        changeInterval = UnityEngine.Random.Range(minChangeInterval, maxChangeInterval);
    }
}
