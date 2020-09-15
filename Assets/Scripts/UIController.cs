using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Ability Components")]
    [SerializeField] AbilityLoadout _loadoutScript = null;
    [SerializeField] Image _abilityImage = null;
    [SerializeField] Image _fillImage = null;
    [SerializeField] Text _abilityText = null;
    [SerializeField] Sprite _defaultIcon = null;
    

    [Header("HealthComponents")]
    [SerializeField] Health _playerHealth = null;
    [SerializeField] Slider _healthSlider = null;
    [SerializeField] Image _healthSliderFill = null;
    [SerializeField] Color _damageColor;
    [SerializeField] Text _healthText = null;
    [SerializeField] Text _changeText = null;

    #region subscriptions
    private void OnEnable()
    {
        _loadoutScript.SetAbility += OnEquip;
        _loadoutScript.Cooldown += UpdateCooldown;
        _playerHealth.HealthSet += UpdateHealthSlider;
        _playerHealth.TookDamage += DamageFeedback;
        _playerHealth.HealthRestored += HealFeedback;
    }

    private void OnDisable()
    {
        _loadoutScript.SetAbility -= OnEquip;
        _loadoutScript.Cooldown -= UpdateCooldown;
        _playerHealth.HealthSet -= UpdateHealthSlider;
        _playerHealth.TookDamage -= DamageFeedback;
        _playerHealth.HealthRestored -= HealFeedback;
    }
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        _abilityText.text = "None";
        _abilityImage.sprite = _defaultIcon;
        _fillImage.fillAmount = 0;
        _fillImage.color = Color.white;

        _healthSlider.minValue = 0;
        _healthSlider.maxValue = _playerHealth.MaxHealth;
        UpdateHealthSlider(_playerHealth.CurrentHealth);
    }


    private void OnEquip(string name, Color color, Sprite sprite)
    {
        _abilityText.text = name;
        _fillImage.color = color;
        _fillImage.fillAmount = 1;
        _abilityImage.sprite = sprite;
    }


    private void UpdateCooldown(float currentCooldown, float totalCooldown)
    {
        _fillImage.fillAmount = 1 - (currentCooldown / totalCooldown);
    }


    private void UpdateHealthSlider(int healthToSet)
    {
        _healthSlider.value = healthToSet;
        _healthText.text = healthToSet.ToString() + " / " + _playerHealth.MaxHealth;
    }

    private void DamageFeedback(int damageAmount)
    {
        // start coroutine to flash the bar red for a short time
    }

    private void HealFeedback(int healAmount)
    {
        // something with the numbers here
    }
}
