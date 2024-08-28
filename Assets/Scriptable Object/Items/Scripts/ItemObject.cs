using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : ScriptableObject
{
    public enum itemType
    {
        Consumable,
        Equipement,
        QuestItem
    }

    public string Name;
    public itemType type;
    public GameObject image;
    [TextArea(15,20)]
    public string Description;


}
