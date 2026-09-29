using UnityEngine;

public class HammerTheGap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private chatemplate player;
    private int BonusDamage = 0; 
    void Awake()
    {
        player = GetComponent<chatemplate>();       
    }

    // Update is called once per frame
    void OnEnable()
    {
        EventMap.PlayerAfterAttackEvent += HammerTheGapEffect;       
    }

    void OnDisable()
    {
        EventMap.PlayerAfterAttackEvent -= HammerTheGapEffect;
    }

    void HammerTheGapEffect(chatemplate attacker, chatemplate target, bool isHit)
    {
        if (attacker != player) return;
        if (isHit)
        {
            BonusDamage++; 
            attacker.dmg += BonusDamage;
            Debug.Log("Hammer The Gap activated! Damage roll is: " + attacker.dmg);
        }
        else
        {
            attacker.dmg -= BonusDamage;
            BonusDamage = 0;
            Debug.Log("Hammer The Gap reset. Damgage roll is"  + attacker.dmg);    
        }
    }
}
