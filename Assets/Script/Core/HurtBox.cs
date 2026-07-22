using System.Resources;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    Stat stat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GetComponentInParent<PlayerStatManager>() != null)
            stat = GetComponentInParent<PlayerStatManager>();
        else if (GetComponentInParent<EnemyStatManager>() != null)
            stat = GetComponentInParent<EnemyStatManager>();

        Debug.Log(stat.gameObject.name);
    }


    public void TakeDamage(float damage)
    { 
        stat.TakeDamage(damage);
    }
}
