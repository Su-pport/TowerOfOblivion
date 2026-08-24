using UnityEngine;

[CreateAssetMenu(menuName="Game/Item/Accessory")]
public class AccessoryItem : Item
{
    private void OnEnable() {
        itemType = ItemType.Accessory;
    }
}
