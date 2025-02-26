using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private List<Item> items = new List<Item>();
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private ItemButtonManager itemButtonManager;


    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.OnItemsMenu += CreateButtons;
    }

    private void CreateButtons()
    {
        ClearButtons();
        foreach (var item in items)
        {
            ItemButtonManager itemButton;
            itemButton = Instantiate(itemButtonManager, buttonContainer.transform);
            itemButton.ItemName = item.ItemName;
            itemButton.ItemImage = item.ItemImage;
            itemButton.Item3DModel = item.Item3DModel;
            itemButton.name = item.ItemName;
        }
        GameManager.instance.OnItemsMenu -= CreateButtons;
    }

    public void CreateSpecificButton(string itemNameToActivate)
    {
        ClearButtons();
        foreach (var item in items)
        {
            if (item.ItemName.Equals(itemNameToActivate, StringComparison.OrdinalIgnoreCase))
            {
                ItemButtonManager itemButton = Instantiate(itemButtonManager, buttonContainer.transform);
                itemButton.ItemName = item.ItemName;
                itemButton.ItemImage = item.ItemImage;
                itemButton.Item3DModel = item.Item3DModel;
                itemButton.name = item.ItemName;
                Debug.LogError(itemButton.name);
            }
        }
    }

    private void ClearButtons()
    {
        foreach (Transform child in buttonContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
