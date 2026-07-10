using UnityEngine;

public class BrainUI : MonoBehaviour
{
    
    public RectTransform hpFill;
    public RectTransform mpFill;

    [Range(0,1)]
    public float hpPercent = 1f;

    [Range(0,1)]
    public float mpPercent = 1f;

    private float hpStartHeight;
    private float mpStartHeight;

    void Start()
    {
        hpStartHeight = hpFill.sizeDelta.y;
        mpStartHeight = mpFill.sizeDelta.y;
    }

    // Update is called once per frame
    void Update()
    {
        hpFill.sizeDelta =
            new Vector2(
                hpFill.sizeDelta.x,
                hpStartHeight * hpPercent);

        mpFill.sizeDelta =
            new Vector2(
                mpFill.sizeDelta.x,
                mpStartHeight * mpPercent);
    }
}
