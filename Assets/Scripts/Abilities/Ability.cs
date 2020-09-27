using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Abilities")]
public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public Color abilityColor;
    public Sprite abilitySprite;
    public float castTime;
    public float duration;
    public float cooldown;
    public AudioClip startSound;
    public AudioClip activeSound;

    public abstract void Setup();
    public abstract void Use(Transform origin, Transform target);
}
