using UnityEngine;

public class Enemy : MonoBehaviour
{
    /*
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    */
    public GameObject targetUnit;   //상대 오브젝트
    public float enemySpeed = 1.0f; //내 속도
    public float desiredDistance = 1.0f; // 상대랑 유지할 거리(근거리, 원거리)
    // Update is called once per frame
    void Update()
    {
        if (targetUnit != null)
        {
            Vector2 targetPos = (Vector2)targetUnit.transform.position; // 현재 상대 위치

            float distance = Vector2.Distance(transform.position, targetPos); //현재 나와 상대의 거리 계산 

            if(distance > desiredDistance) //나와 상대와의 거리가 유지할 거리보다 클 경우
            {
                //이동하고 도착하면 스무스하게 멈춤
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPos,
                    enemySpeed * Time.deltaTime
                );
            }
            
        }
    }
}
