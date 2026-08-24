using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public InventoryAreaUI inventoryAreaUI; // 상위 오브젝트 저장


    private Item currentItem; // 이 슬롯이 가지고 있는 아이템을 저장하는 변수
    [SerializeField] private Image iconImage; // 이 슬롯에 표시할 아이콘 오브젝트

    //버튼이 눌렸을때 사용하는 변수
    private void Awake() {
        Button btn = this.GetComponent<Button>();
        inventoryAreaUI = GetComponentInParent<InventoryAreaUI>();

        btn.onClick.AddListener(SelectItem);
    }
    
    public void SetItem(Item item)
    {
        iconImage.color = new Color(1,1,1,1);
        currentItem = item;
        iconImage.sprite = item.icon;
        if(iconImage.sprite != null && item.itemCode < 300)
            iconImage.transform.localRotation = Quaternion.Euler(0, 0, -45);
    }

    // 인벤토리 빈 칸 알파값 0으로 하여 투명화
    public void Clear()
    {
        iconImage.color = new Color(1,1,1,0);
    }

    public void SelectItem()
    {
        if(currentItem!=null)
            inventoryAreaUI.ClickedSlot(currentItem);
    }
    


}
