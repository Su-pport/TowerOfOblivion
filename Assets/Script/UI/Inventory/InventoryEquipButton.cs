using UnityEngine;
using UnityEngine.UI;


public class InventoryEquipButton : MonoBehaviour
{
    [SerializeField] private InventoryDescAreaUI inventoryDescAreaUI; // 상위 오브젝트

    private void Awake()
    {
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(EquipItem);
    }

    private void EquipItem()
    {
        inventoryDescAreaUI.ClickedEquipButton();
    }
}
