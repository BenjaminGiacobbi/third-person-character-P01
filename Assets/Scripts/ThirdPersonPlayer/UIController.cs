using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ThirdPersonMovement))]
[RequireComponent(typeof(AbilityLoadout))]
[RequireComponent(typeof(Health))]
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
    [SerializeField] Text _healthText = null;
    [SerializeField] Text _changeText = null;

    [Header("DamageComponents")]
    [SerializeField] Image _damageWindow = null;
    [SerializeField] Color _damageColor;
    [SerializeField] float _damageFlashTime = 0;
    [SerializeField] float _lowHealthBenchmark1 = 0.3f, _lowHealthBenchmark2 = 0.2f, _lowHealthBenchmark3 = 0.1f;
    [SerializeField] float _windowXScaleMargin = 0.5f, _windowYScaleMargin = 1f;
    [SerializeField] AudioClip _heartbeatAudio = null;

    ThirdPersonMovement _movementScript = null; 
    AbilityLoadout _loadoutScript = null;
    Health _playerHealth = null;

    float _maxWindowXScale = 1;
    float _maxWindowYScale = 1;

    Coroutine _damageCoroutine = null;
    Coroutine _windowCoroutine = null;

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

        _maxWindowXScale = 1 + _windowXScaleMargin * 4;
        _maxWindowYScale = 1 + _windowYScaleMargin * 4;
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

        if (_windowCoroutine != null)
            StopCoroutine(_windowCoroutine);
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
        UpdateDamageWindow(healthToSet);
    }

    // manages the damage window health effect // TODO kinda disgusting, fix
    private void UpdateDamageWindow(int currentHealth)
    {
        if(_windowCoroutine != null)
            StopCoroutine(_windowCoroutine);

        // these conversions are listed as redundant, but this does integer division otherwise
        float healthPercentage = (float)currentHealth / (float)_playerHealth.MaxHealth;
        if (healthPercentage > _lowHealthBenchmark1)
        {
            _damageWindow.gameObject.SetActive(false);
            _damageWindow.gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            float marginMultiplier = 0;

            if (healthPercentage > _lowHealthBenchmark2)
                marginMultiplier = 3f;
            else if (healthPercentage > _lowHealthBenchmark3)
                marginMultiplier = 2f;
            else
                marginMultiplier = 1f;

            _damageWindow.gameObject.transform.localScale = new Vector3
                (1 + _windowXScaleMargin * marginMultiplier, 1 + _windowYScaleMargin * marginMultiplier, 1);

            _windowCoroutine = StartCoroutine(PulseHealthWindow(_damageWindow.gameObject.transform.localScale, 4f - marginMultiplier));
            _damageWindow.gameObject.SetActive(true);
        }
    }

    private void DamageFeedback(int damageAmount)
    {
        _damageCoroutine = null;
        _damageCoroutine = StartCoroutine(HealthBarFlash());
    }

    private void HealFeedback(int healAmount)
    {
        // something with the numbers here
    }

    IEnumerator HealthBarFlash()
    {
        Color tempColor = _healthSliderFill.color;
        _healthSliderFill.color = _damageColor;
        yield return new WaitForSeconds(_damageFlashTime);
        _healthSliderFill.color = tempColor;
        _damageCoroutine = null;
    }

    IEnumerator PulseHealthWindow(Vector3 currentScale, float beatModifier)
    {
        float minX = currentScale.x;
        float minY = currentScale.y;

        while(true)
        {
            AudioHelper.PlayClip2D(_heartbeatAudio, 0.25f * beatModifier);
            Debug.Log("Running");
            while (currentScale.x < _maxWindowXScale && currentScale.y < _maxWindowYScale)
            {
                currentScale.x = BasicCounter.TowardsTarget(currentScale.x, _maxWindowXScale, _windowXScaleMargin * beatModifier * 3f);
                currentScale.y = BasicCounter.TowardsTarget(currentScale.y, _maxWindowYScale, _windowYScaleMargin * beatModifier * 3f);

                _damageWindow.gameObject.transform.localScale = new Vector3(currentScale.x, currentScale.y, 1);
                yield return null;
            }

            yield return new WaitForSeconds(0.25f - (0.05f * beatModifier));

            while (currentScale.x > minX && currentScale.y > minY)
            {
                currentScale.x = BasicCounter.TowardsTarget(currentScale.x, minX, _windowXScaleMargin * beatModifier * 3f);
                currentScale.y = BasicCounter.TowardsTarget(currentScale.y, minY, _windowYScaleMargin * beatModifier * 3f);

                _damageWindow.gameObject.transform.localScale = new Vector3(currentScale.x, currentScale.y, 1);
                yield return null;
            }
        }
    }
}
