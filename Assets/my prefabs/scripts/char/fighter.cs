using UnityEngine;

[RequireComponent(typeof(HammerTheGap))]
public class fighter : chatemplate
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hp = 10;
        attack = 7;
        defense = 15;
        dmg = 10;
        speed_max = 5;
        hit_range = 3;
        character_name = "Fighter";
        unique_ability = "Hammer The Gap";
    }
    
   
}
