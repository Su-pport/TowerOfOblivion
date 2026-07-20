using System.Resources;
using TMPro;
using UnityEngine;

public class StatUI : MonoBehaviour
{
    public PlayerStatManager playerStatManager;

    public TMP_Text pointText; //남은 포인트 텍스트 오브젝트

    public TMP_Text attackText; //공격력 텍스트 오브젝트
    public TMP_Text magicText; //마력 텍스트 오브젝트
    public TMP_Text agilityText; //순발력 텍스트 오브젝트
    public TMP_Text healthText; //체력 텍스트 오브젝트
    public TMP_Text willText; //정신력 텍스트 오브젝트
    public TMP_Text staminaText; //지구력 텍스트 오브젝트

    int tempAttack; //적용하기 전 찍은 공격력 포인트
    int tempMagic; //적용하기 전 찍은 마력 포인트
    int tempAgility; //적용하기 전 찍은 순발력 포인트
    int tempHealth; //적용하기 전 찍은 체력 포인트
    int tempWill; //적용하기 전 찍은 정신력 포인트
    int tempStamina; //적용하기 전 찍은 지구력 포인트

    int tempPoint; //적용하기 전 남은 포인트

    void Start()
    {
        ResetTempStat(); //적용하기 전 포인트 리셋
    }
    void Update()
    {
        pointText.text = $"남은 포인트 : {playerStatManager.statPoint} (-{tempPoint})";

        attackText.text = $"{playerStatManager.statAttackPower} (+{tempAttack})";
        magicText.text = $"{playerStatManager.statMagicPower} (+{tempMagic})";
        agilityText.text = $"{playerStatManager.statAgility} (+{tempAgility})";
        healthText.text = $"{playerStatManager.statHealth} (+{tempHealth})";
        willText.text = $"{playerStatManager.statWillPower} (+{tempWill})";
        staminaText.text = $"{playerStatManager.statStamina} (+{tempStamina})";
    }

    public void ResetTempStat()
    {
        tempAttack = 0;
        tempMagic = 0;
        tempAgility = 0;
        tempHealth = 0;
        tempWill = 0;
        tempStamina = 0;

        tempPoint = 0;
    }

    public void AddAttack()
    {
        
        if (playerStatManager.statPoint <= tempPoint) return;

        tempAttack++;
        tempPoint++;

    }

    public void AddMagic()
    {
        if (playerStatManager.statPoint <= tempPoint) return;

        tempMagic++;
        tempPoint++;
    }

    public void AddAgility()
    {
        if (playerStatManager.statPoint <= tempPoint) return;

        tempAgility++;
        tempPoint++;
    }

    public void AddHealth()
    {
        if (playerStatManager.statPoint <= tempPoint) return;

        tempHealth++;
        tempPoint++;
    }

    public void AddWill()
    {
        if (playerStatManager.statPoint <= tempPoint) return;

        tempWill++;
        tempPoint++;
    }

    public void AddStamina()
    {
        if (playerStatManager.statPoint <= tempPoint) return;

        tempStamina++;
        tempPoint++;
    }

    public void RemoveAttack()
    {
        if (tempAttack <= 0) return;

        tempAttack--;
        tempPoint--;
    }

        public void RemoveMagic()
    {
        if (tempMagic <= 0) return;

        tempMagic--;
        tempPoint--;
    }

        public void RemoveAgility()
    {
        if (tempAgility <= 0) return;

        tempAgility--;
        tempPoint--;
    }

        public void RemoveHealth()
    {
        if (tempHealth <= 0) return;

        tempHealth--;
        tempPoint--;
    }

        public void RemoveWill()
    {
        if (tempWill <= 0) return;

        tempWill--;
        tempPoint--;
    }

        public void RemoveStamina()
    {
        if (tempStamina <= 0) return;

        tempStamina--;
        tempPoint--;
    }

    public void ApplyStat()
    {
        playerStatManager.statAttackPower += tempAttack;
        playerStatManager.statMagicPower += tempMagic;
        playerStatManager.statAgility += tempAgility;
        playerStatManager.statHealth += tempHealth;
        playerStatManager.statWillPower += tempWill;
        playerStatManager.statStamina += tempStamina;

        playerStatManager.statPoint -= tempPoint;

        ResetTempStat();
    }

    public void CancelStat()
    {
        ResetTempStat();
    }

}
