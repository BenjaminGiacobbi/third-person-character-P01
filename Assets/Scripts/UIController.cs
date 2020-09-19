using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] Canvas UIParent = null;

    [Header("Ability Components")]
    [SerializeField] Image _abilityImage = null;
    [SerializeField] Image _fillImage = null;
    [SerializeField] Text _abilityText = null;
    [SerializeField] Sprite _defaultIcon = null;
    

    [Header("HealthComponents")]
    [SerializeField] Slider _healthSlider = null;
    [SerializeField] Image _healthSliderFill = null;
    [SerializeField] Color _damageColor;
    [SerializeField] Text _healthText = null;
    [SerializeField] Text _changeText = null;

    ThirdPersonMovement _movementScript = null; 
    AbilityLoadout _loadoutScript = null;
    Health _playerHealth = null;

    private void Awake()
    {
        _movementScript = GetComponent<ThirdPersonMovement>();
        _loadoutScript = GetComponent<AbilityLoadout>();
        _playerHealth = GetComponent<Health>();
    }

    #region subscriptions
    private void OnEnable()
    {
        _movementScript.Active += CreateUI;
        _movementScript.Inactive += HideUI;
        
    }

    private void OnDisable()
    {
        _movementScript.Active -= CreateUI;
        _movementScript.Inactive -= HideUI;
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


    void CreateUI()
    {
        Debug.Log("Create");
        UIParent.gameObject.SetActive(true);

        _loadoutScript.SetAbility += OnEquip;
        _loadoutScript.Cooldown += UpdateCooldown;
        _playerHealth.HealthSet += UpdateHealthSlider;
        _playerHealth.TookDamage += DamageFeedback;
        _playerHealth.HealthRestored += HealFeedback;

        UpdateHealthSlider(_playerHealth.CurrentHealth);
    }

    void HideUI()
    {
        Debug.Log("Hide");
        UIParent.gameObject.SetActive(false);

        _loadoutScript.SetAbility -= OnEquip;
        _loadoutScript.Cooldown -= UpdateCooldown;
        _playerHealth.HealthSet -= UpdateHealthSlider;
        _playerHealth.TookDamage -= DamageFeedback;
        _playerHealth.HealthRestored -= HealFeedback;
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
