using UnityEngine;

[CreateAssetMenu(menuName="Game/Item/Weapon")]
public class WeaponItem : Item {
    private void OnEnable() {
        itemType = ItemType.Weapon;
    }
    
    public int attackPower;
    public int magitPower;
    public float attackSpeed;
}
