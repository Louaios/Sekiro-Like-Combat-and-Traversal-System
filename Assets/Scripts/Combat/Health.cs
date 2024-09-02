using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 100;

    [SerializeField]  private int health;

    [SerializeField] private int _maxPosture = 100;

    [SerializeField] private int postureDecreaseRate = 15;

    public float posture = 0;

    [SerializeField] private AudioSource blockingAudio;
    [SerializeField] private AudioSource deflectingAudio;

    public event Action OnTakeDamage;

    public event Action OnPostureDamage;

    public event Action OnDieEvent;

    public event Action OnBreakEvent;

    private bool isInvunerable;
    [field: SerializeField] public bool isParrying {  get; private set; }
    public bool isDead => health == 0;

    [SerializeField] private Slider healthSlider;
    public Slider postureSlider;
    public Slider mirrorPostureSlider;
    void Start()
    {
        health = _maxHealth;
        healthSlider.maxValue = _maxHealth;
        healthSlider.value = _maxHealth;
        postureSlider.maxValue = _maxPosture;
        postureSlider.value = 0;
        mirrorPostureSlider.maxValue = _maxPosture;
        mirrorPostureSlider.value = 0;
    }
    private void Update()
    {
        if(posture > 0)
          PostureDecrease(postureDecreaseRate * Time.deltaTime);

        if (posture >= _maxPosture)
        {
            OnBreakEvent?.Invoke();
            //TO emplement breakingState we will subscribe to this event through StateMachine
        }
    }

    public void SetInvulnerability(bool enable)
    {
        isInvunerable = enable;
    }

    public void DealDamage(int damage)
    {
        if (health == 0) { return; }

        if(isInvunerable && !isParrying) 
        {
            blockingAudio.Play();
            posture = Mathf.Max(posture + damage, 0);
            postureSlider.value = posture;
            mirrorPostureSlider.value = posture;
            return;
        }

        if (isParrying)
        {
            deflectingAudio.Play();
            OnPostureDamage?.Invoke();
            return;
        }

        health = Mathf.Max(health - damage, 0);

        healthSlider.value = health;
        posture = Mathf.Max(posture + damage, 0);
        postureSlider.value = posture;
        mirrorPostureSlider.value = posture;

        OnTakeDamage?.Invoke();

        if (posture >= _maxPosture)
        {
            OnBreakEvent?.Invoke();
            //TO emplement breakingState we will subscribe to this event through StateMachine
        }

        if (health == 0)
        {
            OnDieEvent?.Invoke();
        }
      // Debug.Log(health); Will use UI to display damage dealth
    }

    private void PostureDecrease(float rate)
    {
        posture -= rate;
        postureSlider.value = posture;
        mirrorPostureSlider.value= posture;

        if (posture < 0) posture = 0;
    }

    public void SetParryTrue()
    {
        isParrying = true;
    }

    public void SetParryFalse()
    {
        isParrying = false;
    }

    public void SetUI(bool enable)
    {
         healthSlider.gameObject?.SetActive(enable);
        postureSlider.gameObject?.SetActive(enable);
        mirrorPostureSlider.gameObject?.SetActive(enable);
    }
}
