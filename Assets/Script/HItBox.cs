using UnityEngine;

public class HitBox : MonoBehaviour
{

    public LayerMask layermask;
    public HurtBox hurtbox;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(layermask == (layermask | (1 << other.gameObject.layer)))
        {
            hurtbox = other.GetComponent<HurtBox>();

            if (hurtbox != null)
                Onhit(hurtbox);
        }
    }

    private void Onhit(HurtBox hurtBox)
    {

    }
}
