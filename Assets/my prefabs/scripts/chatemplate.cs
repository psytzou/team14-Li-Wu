using System;
using UnityEngine;

public class chatemplate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Basic Abilities")]
    public int hp;
    public int attack;
    public int defense;
    public int dmg;
    public int speed_max;
    public int hit_range;

    public string character_name = "Default Name";
    public string unique_ability = "None";

    public event Action<chatemplate, chatemplate> Died;

    private chatemplate lastDamageSource;
    private bool deathHandled;

    public void ReceiveDamage(chatemplate source, int amount)
    {
        if (deathHandled || amount < 0)
        {
            return;
        }

        lastDamageSource = source;
        hp -= amount;
    }

   void Update()
    {
        if (hp <= 0 && !deathHandled)
        {
            deathHandled = true;
            Debug.Log(character_name + " has died.");
            Died?.Invoke(this, lastDamageSource);
            Destroy(gameObject); // Handle character death logic
        }
    }


}
