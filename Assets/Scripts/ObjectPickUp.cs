using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPickUp : MonoBehaviour
{
    public InventoryObject inventory;
    public LayerMask objectLayer;
    public KeyCode interactKey = KeyCode.E;

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.5f, objectLayer);
        foreach (Collider collider in colliders)
        {
            Item item = collider.GetComponent<Item>();

            if (item != null)
            {
                inventory.AddItem(item.item, 1);
                Destroy(item.gameObject);
            }
        }
    }

  //private void OnApplicationQuit()
  //{
    //  inventory.Container.Clear();
 // }
}
