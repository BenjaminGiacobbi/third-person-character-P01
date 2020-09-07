using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] Image _abilityImage = null;
    [SerializeField] Image _fillImage = null;
    [SerializeField] Text _abilityText = null;
    [SerializeField] Sprite _defaultIcon = null;
    [SerializeField] AbilityLoadout _loadoutScript = null;

    // Start is called before the first frame update
    void Start()
    {
        _abilityText.text = "None";
        _abilityImage.sprite = _defaultIcon;
        _fillImage.fillAmount = 0;
        _fillImage.color = Color.white;
    }

    #region subscriptions
    private void OnEnable()
    {
        _loadoutScript.SetAbility += OnEquip;
        _loadoutScript.Cooldown += UpdateCooldown;
    }

    private void OnDisable()
    {
        _loadoutScript.SetAbility += OnEquip;
        _loadoutScript.Cooldown += UpdateCooldown;
    }
    #endregion


    public void OnEquip(string name, Color color, Sprite sprite)
    {
        _abilityText.text = name;
        _fillImage.color = color;
        _fillImage.fillAmount = 1;
        _abilityImage.sprite = sprite;
    }

    public void UpdateCooldown(float currentCooldown, float totalCooldown)
    {
        _fillImage.fillAmount = 1 - (currentCooldown / totalCooldown);
    }
}
