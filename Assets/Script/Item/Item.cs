using UnityEngine;

public enum ItemType {
    Weapon,     // 무기 아이템코드 0~99 근접 100~199 활 200~299 마법
    Armor,      // 방어구 아이템코드 300~399 헬멧 400~499 갑옷
    Accessory,   // 장신구 아이템코드 500~599
    Consumable // 소비 아이템 (포션 등) 600~699    

}

[CreateAssetMenu(menuName="Game/Item")]
public abstract class Item : ScriptableObject {
    [Tooltip("0~99: 근접무기, 100~199: 활, 200~299: 마법무기, 300~399: 헬멧, 400~499 갑옷, 500~599: 장신구, 600~699: 소비아이템")]
    public int itemCode; //아이템코드
    [Tooltip("작명법: itemCode_itemName")]
    public string itemName; // 아이템 이름
    public Sprite icon; // 아이템 아이콘
    public string description; // 설명
    public int sellValue; // 판매가격
}