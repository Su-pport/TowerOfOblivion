using UnityEngine;

[CreateAssetMenu(menuName="Game/Item/Armor")]
public class ArmorItem : Item {
    private void OnEnable() {
        itemType = ItemType.Armor;
    }
    public int defensePower;
}