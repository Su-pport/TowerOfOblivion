using UnityEngine;

[CreateAssetMenu(menuName="Game/Item/Consumable")]
public class ConsumableItem : Item {
    private void OnEnable() {
        itemType = ItemType.Consumable;
    }
    public int healAmount;
}
