using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GameManager : MonoBehaviour
{

    public event Action OnMainMenu;
    public event Action OnItemsMenu;
    public event Action OnARPosition;
    public event Action OnQuiz;
    public static GameManager instance;

    private GPSManager gpsManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        MainMenu();
    }

    public void MainMenu()
    {
        OnMainMenu?.Invoke();
        Debug.Log("Main Menu Activated");    }

    public void ItemsMenu()
    {
        OnItemsMenu?.Invoke();
        Debug.Log("Items Menu Activated");
    }
    public void ARPosition()
    {
        OnARPosition?.Invoke();
        Debug.Log("AR Position Activated");

        if (gpsManager != null)
        {
            StartCoroutine(gpsManager.StartLocationService());
        }
    }

    public void Quiz()
    {
        OnQuiz?.Invoke();
        Debug.Log("Quiz Activated");
    }

    public void CloseAPP()
    {
        Application.Quit();
    }
    
}
