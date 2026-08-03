using UnityEngine;

public class EnemyStatManager : Stat
{
    [Header("EnemyStatManger 변수")]
    [SerializeField] private int deathExp;
    [SerializeField] private int dropGoldMin;
    [SerializeField] private int dropGoldMax;
    private int dropGold;


    [SerializeField] private PlayerStatManager playerStatManager;
    [SerializeField] private InventoryManager InventoryManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        init();

        OnDeath+= Death;

        dropGold = UnityEngine.Random.Range(dropGoldMin, dropGoldMax);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Death()
    {
        playerStatManager.AddExp(deathExp);
        InventoryManager.AddGold(dropGold);
        Debug.Log(dropGold+"골드 획득");
    }
}
