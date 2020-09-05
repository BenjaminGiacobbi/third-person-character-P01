using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Ability : ScriptableObject
{
    // public enum AbilityType { Continue, Single };
    // public AbilityType type;
    public string abilityName;
    public float castTime;
    public float duration;
    public float cooldown;
    public AudioClip activeSound;

    public abstract void Setup();
    public abstract void Use(Transform origin, Transform target);   
}
