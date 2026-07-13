using System;
using UnityEngine;

public class StaminaBar : MonoBehaviour
{
    public Stat stat;
    public Transform fill;

    private float startWidth;

    void Start()
    {
        startWidth = fill.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        float ratio =
            stat.currentST /
            stat.maxST;

        fill.localScale =
            new Vector3(
                startWidth * ratio,
                fill.localScale.y,
                fill.localScale.z
            );
    }
    

}
