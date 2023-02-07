using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CharacterStates : MonoBehaviour
{


    public event Action<int, int> UpdateHealthBarOnAttack;
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;

    public AttackData_SO attackData;
    [HideInInspector]
    public bool isCritical;





    private void Awake()
    {
        if(templateData != null){
            characterData = Instantiate(templateData);
        }
    }

    #region Read from Data_SO
    public int MaxHealth
    {
        get
        {
            if (characterData != null)
                return characterData.maxHealth;
            else return 0;
        }
        set
        {
            characterData.maxHealth = value;

        }
    }
    public int CurrentHealth
    {
        get
        {
            if (characterData != null)
                return characterData.currentHealth;
            else return 0;
        }
        set
        {
            characterData.currentHealth = value;

        }
    }
    public int BaseDefence
    {
        get
        {
            if (characterData != null)
                return characterData.baseDefence;
            else return 0;
        }
        set
        {
            characterData.baseDefence = value;

        }
    }
     
    public int CurrentDefence
    {
        get
        {
            if (characterData != null)
                return characterData.currentDefence;
            else return 0;
        }
        set
        {
            characterData.currentDefence = value;

        }
    }
    #endregion

    #region Character Combat
    public void TakeDamage(CharacterStates attacker, CharacterStates defener)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrentDefence,0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        if (attacker.isCritical)
        {

            defener.GetComponent<Animator>().SetTrigger("Hit");
        }
        //TODO:Update UI

        UpdateHealthBarOnAttack?.Invoke(CurrentHealth,MaxHealth);
        //TODO:æ≠—ÈUpdate
        if(CurrentHealth <= 0)
        {
            attacker.characterData.UpdateExp(characterData.killPoint);
        }
    }
    public void TakeDamage(int damage, CharacterStates defener)
    {
        int currentDamage = Mathf.Max(damage - defener.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
    }
        
    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);

        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
            Debug.Log("±©ª˜£°" + coreDamage);
        }
        return (int)coreDamage;
    }


    #endregion
}
