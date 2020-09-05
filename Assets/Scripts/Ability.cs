using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Ability : ScriptableObject
{
    // public enum AbilityType { Continue, Single };
    // public AbilityType type;
    public float abilityCastTime;
    public float abilityDuration;
    public float abilityCooldown;

    public abstract void Setup();

    public abstract void Use(Transform origin, Transform target);

    public abstract void Reset();

    
}
