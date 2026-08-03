using UnityEngine;

public class EnemyStatManager : Stat
{
    [Header("EnemyStatManger 변수")]
    [SerializeField] private int deathExp;


    [SerializeField] private PlayerStatManager playerStatManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        init();

        OnDeath+= Death;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Death()
    {
        playerStatManager.AddExp(deathExp);
    }
}
