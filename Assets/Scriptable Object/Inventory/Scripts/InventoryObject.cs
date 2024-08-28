using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System / Inventory")]
public class InventoryObject : ScriptableObject
{
    public List<InventorySlots> Container = new List<InventorySlots>();
    public void AddItem(ItemObject _item, int _Amount)
    {
        bool hasItem = false;
        for (int i = 0; i < Container.Count; i++)
        {
            InventorySlots slot = Container[i];
            if (_item == slot.item)
            {
                Container[i].AddAmount(_Amount);
                hasItem = true;
                break;
            }
        }
        if (!hasItem)
        {
            Container.Add(new InventorySlots(_item, _Amount));
        }
    }
}

[System.Serializable]
//To make it show in the unity editor when added to a GameObject;
public class InventorySlots
{
    public ItemObject item;
    public int amount;
    public InventorySlots(ItemObject item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
    public void AddAmount(int value)
    { 
        this.amount+= value;
    }
} 
