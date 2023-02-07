using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab;

    public Transform barPoint;
    public bool alwaysVisible;

    public float visibleTime;
    Image healthSlider;
    Transform UIBar;
    private float timeLeft;
    Transform cam;
    CharacterStates currentStates;
    private void Awake()
    {
        currentStates = GetComponent<CharacterStates>();
        currentStates.UpdateHealthBarOnAttack += UpdateHealthBar;
    }
    private void OnEnable()
    {
        cam = Camera.main.transform;
        foreach(var canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                UIBar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIBar.GetChild(0).GetComponent<Image>();
                UIBar.gameObject.SetActive(alwaysVisible);

            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if(currentHealth <= 0)
        {
            Destroy(UIBar.gameObject);

        }
        UIBar.gameObject.SetActive(true);
        timeLeft = visibleTime;
        float sliderPercent = (float)currentHealth / maxHealth;
        healthSlider.fillAmount = sliderPercent;

    }
    private void LateUpdate()
    {
        if(UIBar != null)
        {
            UIBar.position = barPoint.position;
            UIBar.forward = -cam.forward;
            if(timeLeft <= 0 && !alwaysVisible)
            {
                UIBar.gameObject.SetActive(false);
            }
            else
            {
                timeLeft -= Time.deltaTime;
            }
        }
    }
}
