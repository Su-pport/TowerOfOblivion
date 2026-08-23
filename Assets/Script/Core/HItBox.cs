using UnityEngine;

public class HitBox : MonoBehaviour
{

    public LayerMask layermask;
    public HurtBox hurtbox;

    Stat stat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GetComponentInParent<PlayerStatManager>() != null)
            stat = GetComponentInParent<PlayerStatManager>();
        else if (GetComponentInParent<EnemyStatManager>())
            stat = GetComponentInParent<EnemyStatManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(layermask == (layermask | (1 << other.gameObject.layer)))
        {
            hurtbox = other.GetComponent<HurtBox>();
            if (hurtbox != null)
                OnHit(hurtbox);
        }
        
    }

    private void OnHit(HurtBox hurtBox)
    {
        if (stat == null)
        {
            // Try to recover stat reference on hit; if still missing, abort.
            if (GetComponentInParent<PlayerStatManager>() != null)
                stat = GetComponentInParent<PlayerStatManager>();
            else if (GetComponentInParent<EnemyStatManager>() != null)
                stat = GetComponentInParent<EnemyStatManager>();

            if (stat == null)
            {
                Debug.LogWarning("HitBox: stat reference missing, cannot apply damage.", this);
                return;
            }
        }

        hurtBox.TakeDamage(stat.attackPower);
    }
}
