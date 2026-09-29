using UnityEngine;

public static class attack 
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // public static GameObject hitVFX;
    // public static GameObject missVFX;

    public static bool Hit(chatemplate attacker, chatemplate target)
    {
        bool isHit = false;
        if (target != null)
        {
            // Visual feedback is separate from the hit roll and damage logic.
            // Both successful and missed attacks display the same short line.
            AttackVFX.Play(attacker.transform.position, target.transform.position);

            // check whether successful hit
            int hitRoll = rollCheck.Roll(20) + attacker.attack;
            if (hitRoll >= target.defense)
            {
                isHit = true;
                // Instantiate hit VFX at the target's position
                // Object.Instantiate(hitVFX, target.transform.position, Quaternion.identity);
                Cmdmg(attacker, target, attacker.dmg);
            }
            else
            {
                isHit = false;
                // Instantiate miss VFX at the target's position
                // Object.Instantiate(missVFX, target.transform.position, Quaternion.identity);
                Debug.Log(attacker.character_name + " missed the attack on " + target.character_name);
            }
        }
        else
        {
            Debug.Log("No target to hit.");
        }
        return isHit;
    }

    public static void Cmdmg(chatemplate attacker, chatemplate target, int dmg)
    {
        int dmgtaken = rollCheck.Roll(dmg); 
        target.ReceiveDamage(attacker, dmgtaken);
        Debug.Log(target.character_name + " took " + dmgtaken + " damage. Remaining HP: " + target.hp);
    }
}
