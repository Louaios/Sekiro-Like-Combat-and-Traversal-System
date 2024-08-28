using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new QuestItem", menuName = "Inventory System / Items / QuestItem")]

public class QuestItemObject : ItemObject
{
    private void Awake()
    {
        type = itemType.QuestItem;
    }
}
