using System;
using System.Collections.Generic;
using UnityEngine;

// Cross-scene reference for which character was picked. Owns building the
// live CharacterInstance from the registered hero — PlayerController just
// reads SelectedCharacter.Active and assumes it already exists.
public static class SelectedCharacter
{
    public enum CharacterType { Fighter, Ranger, Rogue }

    public static CharacterType Current = CharacterType.Fighter;

    private static GameObject hero;
    private static CharacterInstance active;

    // Built lazily on first access (rather than eagerly inside Register)
    // because templates like fighter.cs set their real numbers in Start(),
    // which hasn't necessarily run yet at the point the hero is registered.
    public static CharacterInstance Active
    {
        get
        {
            if (active == null && hero != null)
                active = new CharacterInstance(hero.GetComponent<chatemplate>());
            return active;
        }
    }

    // Which chatemplate subclass to attach for each selectable type. Ranger
    // and Rogue have no dedicated script yet, so they're left unmapped —
    // Register then leaves whatever chatemplate is already on the hero.
    private static readonly Dictionary<CharacterType, Type> CharacterScripts = new Dictionary<CharacterType, Type>
    {
        { CharacterType.Fighter, typeof(fighter) },
    };

    // Attaches the script matching the current selection (if missing) and
    // remembers this GameObject as the hero the active instance builds from.
    public static void Register(GameObject builtHero)
    {
        hero = builtHero;
        active = null;

        if (!CharacterScripts.TryGetValue(Current, out Type scriptType) || hero.GetComponent(scriptType) != null)
            return;

        chatemplate existing = hero.GetComponent<chatemplate>();
        if (existing != null && existing.GetType() != scriptType)
            UnityEngine.Object.DestroyImmediate(existing);

        hero.AddComponent(scriptType);
    }
}
