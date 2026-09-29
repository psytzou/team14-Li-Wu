using UnityEngine;
using System;

public static class EventMap
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static event Action<chatemplate, chatemplate, bool> PlayerAfterAttackEvent;
    public static void PlayerAfterAttack(chatemplate attacker, chatemplate target, bool isHit) => PlayerAfterAttackEvent?.Invoke(attacker, target, isHit);

    public static event Action<chatemplate> PlayerAbilityActivatedEvent;
    public static void PlayerAbilityActivated(chatemplate player) => PlayerAbilityActivatedEvent?.Invoke(player);

    // Fired when breathe finishes refreshing a character. Abilities that
    // should reset at turn end (not just on a missed attack) subscribe here
    // themselves -- nothing resets automatically just because this fires.
    public static event Action<chatemplate> TurnEndedEvent;
    public static void TurnEnded(chatemplate character) => TurnEndedEvent?.Invoke(character);

}
