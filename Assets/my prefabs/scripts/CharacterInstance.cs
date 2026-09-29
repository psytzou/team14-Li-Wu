// The actual running game figure built from a template (e.g. fighter.cs).
// A template only defines fixed base numbers; this instance separates
// "current" from "max" so the live character can change during play
// (taking damage, being slowed, etc.) without the template itself changing.
public class CharacterInstance
{
    private readonly chatemplate source;

    public int HpMax { get; }
    public int SpeedMax { get; }
    public int Speed { get; set; }

    // Hp reads straight through to the template's chatemplate.hp, since
    // that's the field attack.Cmdmg already mutates on hit — the instance
    // doesn't keep a separate copy that could drift out of sync.
    public int Hp => source.hp;
    public int Attack => source.attack;
    public int Defense => source.defense;
    public int Dmg => source.dmg;
    public int HitRange => source.hit_range;
    public string CharacterName => source.character_name;

    public CharacterInstance(chatemplate source)
    {
        this.source = source;
        HpMax = source.hp;
        SpeedMax = source.speed_max;
        Speed = source.speed_max;
    }
}
