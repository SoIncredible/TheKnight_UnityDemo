using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHealthUI : MonoBehaviour
{
    Text levelText;
    Image healthSlider;
    Image expSlider;
    private void Awake()
    {
        levelText = transform.GetChild(2).GetComponent<Text>();
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(1).GetChild(0).GetComponent<Image>();
    }
    private void Update()
    {
        levelText.text = "Level " + GameManager.Instance.playerStates.characterData.currentLevel.ToString("00");
        UpdateHealth();
        UpdateExp();
    }
    void UpdateHealth()
    {
        float sliderPercent = (float)GameManager.Instance.playerStates.CurrentHealth / GameManager.Instance.playerStates.MaxHealth;
        healthSlider.fillAmount = sliderPercent;
    }
    void UpdateExp()
    {
        float sliderPercent = (float)GameManager.Instance.playerStates.characterData.currentExp / GameManager.Instance.playerStates.characterData.baseExp;
        expSlider.fillAmount = sliderPercent;
    }
}
