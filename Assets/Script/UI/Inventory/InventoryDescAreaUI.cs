using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryDescAreaUI : MonoBehaviour
{
    [SerializeField] Image selectedItemIcon; //선택한 아이템의 이미지가 보이는 곳
    [SerializeField] Transform selectedItemIconTransform; 
    [SerializeField] TextMeshProUGUI DescText;

    private Item selectedItem = null; // InventoryAreaUI에게 선택한 아이템을 받음

    private void Awake() {
        initIcon();
    }

    private void initIcon()
    {
        if(selectedItem ==null)
            selectedItemIcon.color = new Color(1,1,1,0);
        else
        {
            ShowSelectedItemIcon();
        }
    }

    // 인벤토리 슬롯 클릭 시 호출되는 함수
    public void GetSelectedItem(Item item)
    {
        selectedItem = item;
        Debug.Log(item.itemName);
        ShowSelectedItemIcon();
        ShowItemDesc();
    }

    private void ShowSelectedItemIcon()
    {
        if(selectedItem ==null)
            selectedItemIcon.color = new Color(1,1,1,0);
        else
        {
            if (selectedItem.itemCode < 300)
            {
                selectedItemIconTransform.transform.localRotation = Quaternion.Euler(0, 0, -45);
            }
            selectedItemIcon.color = new Color(1,1,1,1);
            selectedItemIcon.sprite = selectedItem.icon;
        }
    }

    private void ShowItemDesc()
    {
        DescText.text = selectedItem.itemName + "\n" + selectedItem.description;
    }


    
}
