using System.Resources;
using TMPro;
using UnityEngine;

public class StatUI : MonoBehaviour
{
    public PlayerStatManager stat;

    public TMP_Text pointText;

    public TMP_Text attackText;
    public TMP_Text magicText;
    public TMP_Text agilityText;
    public TMP_Text healthText;
    public TMP_Text willText;
    public TMP_Text staminaText;

    int tempAttack;
    int tempMagic;
    int tempAgility;
    int tempHealth;
    int tempWill;
    int tempStamina;

    int tempPoint;

    void Start()
    {
        ResetTempStat();
    }
    void Update()
    {
        pointText.text = $"남은 포인트 : {tempPoint}";

        attackText.text = $"{stat.statAttackPower} (+{tempAttack})";
        magicText.text = $"{stat.statMagicPower} (+{tempMagic})";
        agilityText.text = $"{stat.statAgility} (+{tempAgility})";
        healthText.text = $"{stat.statHealth} (+{tempHealth})";
        willText.text = $"{stat.statWillPower} (+{tempWill})";
        staminaText.text = $"{stat.statStamina} (+{tempStamina})";
    }

    public void ResetTempStat()
    {
        tempAttack = 0;
        tempMagic = 0;
        tempAgility = 0;
        tempHealth = 0;
        tempWill = 0;
        tempStamina = 0;

        tempPoint = stat.statPoint;
    }

    public void AddAttack()
    {
        
        if (tempPoint <= 0) return;

        tempAttack++;
        tempPoint--;

    }

    public void AddMagic()
    {
        if (tempPoint <= 0) return;

        tempMagic++;
        tempPoint--;
    }

    public void AddAgility()
    {
        if (tempPoint <= 0) return;

        tempAgility++;
        tempPoint--;
    }

    public void AddHealth()
    {
        if (tempPoint <= 0) return;

        tempHealth++;
        tempPoint--;
    }

    public void AddWill()
    {
        if (tempPoint <= 0) return;

        tempWill++;
        tempPoint--;
    }

    public void AddStamina()
    {
        if (tempPoint <= 0) return;

        tempStamina++;
        tempPoint--;
    }

    public void RemoveAttack()
    {
        if (tempAttack <= 0) return;

        tempAttack--;
        tempPoint++;
    }

        public void RemoveMagic()
    {
        if (tempMagic <= 0) return;

        tempMagic--;
        tempPoint++;
    }

        public void RemoveAgility()
    {
        if (tempAgility <= 0) return;

        tempAgility--;
        tempPoint++;
    }

        public void RemoveHealth()
    {
        if (tempHealth <= 0) return;

        tempHealth--;
        tempPoint++;
    }

        public void RemoveWill()
    {
        if (tempWill <= 0) return;

        tempWill--;
        tempPoint++;
    }

        public void RemoveStamina()
    {
        if (tempStamina <= 0) return;

        tempStamina--;
        tempPoint++;
    }

    public void ApplyStat()
    {
        stat.statAttackPower += tempAttack;
        stat.statMagicPower += tempMagic;
        stat.statAgility += tempAgility;
        stat.statHealth += tempHealth;
        stat.statWillPower += tempWill;
        stat.statStamina += tempStamina;

        stat.statPoint = tempPoint;

        ResetTempStat();
    }

    public void CancelStat()
    {
        ResetTempStat();
    }

}
