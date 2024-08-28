using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] KeyCode DisplayKey = KeyCode.Tab;
    bool isDisplayed;
    [SerializeField] GameObject inventory;

    private void Awake()
    {
        isDisplayed = false;
        inventory.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(DisplayKey))
        {
            ToggleInventory();
        }
            
    }

    public void ToggleInventory()
    {
        isDisplayed = !isDisplayed;
        inventory.gameObject.SetActive(isDisplayed);
    }

    
}
