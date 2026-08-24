using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public enum EquipResult
{
    StatNotEnough,
    WeaponEquipped,
    HelmetEquipped,
    ChestPlateEquipped,
    AccessoryEquipped,
    NotEquipment,
    Unknown
}

public class InventoryManager : MonoBehaviour {
    public InventoryAreaUI inventoryAreaUI;


    public List<Item> items = new List<Item>();

    private int currentMoney = 0;

    // 장비 슬롯
    public Item equippedWeapon;
    public Item equippedHelmet;
    public Item equippedChestPlate;
    public Item equippedAccessory;

    [SerializeField] private int maxInventorySize = 42;
    public int currentItemCount = 0;


    private void Start()
    {
        // InitInventory();
        inventoryAreaUI.CreateButtons(maxInventorySize);
    }

    //임시코드
    public Item sword1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {

            AddItem(sword1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            AddMaxInventorySize(10);
        }

    }

    // 여 기 까 지 임 시 코 드


    // private void InitInventory()
    // {
    //     for ( int i = 0; i< maxInventorySize; i++)
    //     {
    //         items.Add(null);
    //     }
    // }

    public void AddMaxInventorySize(int size)
    {
        maxInventorySize += size;
        inventoryAreaUI.AddButtons(size);
    }


    public void AddItem(Item item) {
        if( currentItemCount < maxInventorySize) {
            items.Add(item);
            currentItemCount++;

            inventoryAreaUI.RefreshInventory();
        }
    }

    // 골드 관리
    public void AddGold(int amount)
    {
        currentMoney += amount;
    }

    // 아이템 장착 
    public EquipResult EquipItem(Item item)
    {
        if(item.itemType!= ItemType.Weapon && item.itemType != ItemType.Armor && item.itemType != ItemType.Accessory)
            return EquipResult.NotEquipment;

        if(!CheckRequireStat())
            return EquipResult.StatNotEnough;

        switch (item.itemType)
        {
            case ItemType.Weapon:
                equippedWeapon = item;
                return EquipResult.WeaponEquipped;

            case ItemType.Armor:
                if (300 <= item.itemCode && item.itemCode < 400)
                {
                    equippedHelmet = item;
                    return EquipResult.HelmetEquipped;
                }
                else if (item.itemCode < 500)
                {
                    equippedChestPlate = item;
                    return EquipResult.ChestPlateEquipped;
                }
                else
                    return EquipResult.Unknown;
            
            case ItemType.Accessory:
                equippedAccessory = item;
                return EquipResult.AccessoryEquipped;

            default:
                return EquipResult.Unknown;
        }
        
    }

    private bool CheckRequireStat() // 요구스탯 확인
    {
        return true;
    }

}