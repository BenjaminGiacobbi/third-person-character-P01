using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public float abilityDuration;
    public float abilityCooldown;

    public abstract void Use(Transform origin, Transform target);

    public abstract void Reset();
}
