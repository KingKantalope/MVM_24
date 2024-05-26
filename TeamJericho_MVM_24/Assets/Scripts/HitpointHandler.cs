using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHitpoints : MonoBehaviour, IDamageable
{
    [SerializeField] private Actor thisActor;

    public UnityEvent<float, float> OnChangeHealth;
    public UnityEvent<float, float, int, int> OnChangeArmor;
    public UnityEvent<float, float> OnChangeShields;
    public UnityEvent<Vector3, float> OnReceiveDamage;
    public UnityEvent OnDeath;

    #region Variables
    [Header("Shields Hitpoints")]
    [SerializeField] private float startingShields;
    [SerializeField] private float maxShields;
    [SerializeField] private float rechargeDelay;
    [SerializeField] private float rechargeRate;
    private float currentShields;
    private float rechargeTime;
    private bool isRecharging;

    [Header("Armor Hitpoints")]
    [SerializeField] private float startingArmor;
    [SerializeField] private float maxArmor;
    [SerializeField] private int protectionLevel;
    private float currentArmor;

    [Header("Health Hitpoints")]
    [SerializeField] private float startingHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxSelfHeal;
    [SerializeField] private float healDelay;
    [SerializeField] private float healRate;
    [SerializeField] private bool canBeInstakilled;
    private float currentHealth;
    private float amountHealed;
    private float healTime;
    private bool isHealing;

    [Header("Poise")]
    [SerializeField] private float maxPoise;
    [SerializeField] private float recoveryDelay;
    [SerializeField] private float recoveryRate;
    private float currentPoise;
    private float recoveryTime;

    [Header("Radiation")]
    [SerializeField] private float maxRadiation;
    [SerializeField] private float decayDelay;
    [SerializeField] private float decayRate;
    [SerializeField] private int weakenThreshold;
    private float currentRadiation;
    private float decayTime;
    private float radiationEffect;

    private float netRechargeRate;
    private int armorWeakening;
    private float netArmorResilience;

    [Header("Frost")]
    [SerializeField] private float maxFrost;
    [SerializeField] private float meltDelay;
    [SerializeField] private float meltRate;
    [SerializeField] private int staminaThreshold;
    private float currentFrost;
    private float meltTime;
    private float FrostEffect;

    private int staminaLoss;

    [Header("Hemorrhage")]
    [SerializeField] private Hemorrhage hemorrhageLevel;
    [SerializeField] private float bleedDamageMajor;
    [SerializeField] private float bleedDamageMinor;
    [SerializeField] private int numTicksMajor;
    [SerializeField] private int numTicksMinor;
    [SerializeField] private float sutureTick;
    private int numTicksCurrent;
    private float sutureTime;

    [Header("Shock")]
    [SerializeField] private float shockDPS;
    private bool isShocked;

    private bool hasDied;
    #endregion Variables

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = startingHealth;
        OnChangeHealth?.Invoke(currentHealth, maxHealth);
        currentArmor = startingArmor;
        OnChangeArmor?.Invoke(currentArmor, maxArmor, protectionLevel, armorWeakening);
        currentShields = startingShields;
        OnChangeShields?.Invoke(currentShields, maxShields);

        hasDied = false;
        isShocked = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleRadiation();
        HandleFrost();
        HandleHemorrhaging();
        HandleShock();
        HandlePoise();
        HandleHealth();
        HandleShields();

    }

    #region status effects

    private void HandleRadiation()
    {
        netRechargeRate = rechargeRate * (100f - currentRadiation) / 100f;
        armorWeakening = (int)currentRadiation / weakenThreshold;
        netArmorResilience = (100f + currentRadiation) / 100f;

        // decay
        decayTime -= Time.deltaTime;

        if (decayTime <= 0f && currentRadiation > 0f)
        {
            currentRadiation -= decayRate * Time.deltaTime;
        }
    }

    private void HandleFrost()
    {
        staminaLoss = (int)currentFrost / staminaThreshold;

        // melt
        meltTime -= Time.deltaTime;

        if (meltTime <= 0f && currentFrost > 0f)
        {
            currentFrost -= meltRate * Time.deltaTime;
        }
    }

    private void HandleHemorrhaging()
    {
        // bleed ticks
        switch (hemorrhageLevel)
        {
            case Hemorrhage.major:
                sutureTime += Time.deltaTime;
                if (sutureTime <= 0f)
                {
                    sutureTime = sutureTick;
                    currentHealth -= bleedDamageMajor;
                    healTime = healDelay;
                    isHealing = true;
                    OnChangeHealth?.Invoke(currentHealth, maxHealth);

                    if (numTicksCurrent < numTicksMajor) numTicksCurrent++;
                    else hemorrhageLevel = Hemorrhage.none;
                }
                break;
            case Hemorrhage.minor:
                sutureTime += Time.deltaTime;
                if (sutureTime <= 0f)
                {
                    sutureTime = sutureTick;
                    currentHealth -= bleedDamageMinor;
                    healTime = healDelay;
                    isHealing = true;
                    OnChangeHealth?.Invoke(currentHealth, maxHealth);

                    if (numTicksCurrent < numTicksMinor) numTicksCurrent++;
                    else hemorrhageLevel = Hemorrhage.none;
                }
                break;
        }
    }

    private void HandleShock()
    {
        if (isShocked)
        {
            // damage shields
            currentShields -= shockDPS * Time.deltaTime;
            currentShields += netRechargeRate * Time.deltaTime;

            rechargeTime = rechargeDelay;
            healTime = healDelay;
        }
    }

    private void HandlePoise()
    {
        // no idea what to do here lol
    }

    private void HandleHealth()
    {
        if (currentHealth > 0f)
        {
            healTime -= Time.deltaTime;

            if (healTime <= 0f && amountHealed < maxSelfHeal && currentHealth < (maxHealth - currentRadiation))
            {
                currentHealth += healRate * Time.deltaTime;
                amountHealed += healRate * Time.deltaTime;

                OnChangeHealth?.Invoke(currentHealth, maxHealth);
            }
        }
        else
        {
            // call death event
            // this should report to the game that the respawn or reset sequence should be enacted
            // maybe this could just be a call to the Actor class that then deals with it from there?
            // each Actor has its own needs for when handling death, after all
            // but invoking will allow the HUD to do its own death sequence
            // and force other scripts to shut off and such, like movement and aim, at least for singleplayer
            OnDeath?.Invoke();
        }
    }

    private void HandleShields()
    {
        rechargeTime -= Time.deltaTime;

        if (rechargeTime <= 0f && currentShields < maxShields)
        {
            currentShields += netRechargeRate * Time.deltaTime;

            OnChangeShields?.Invoke(currentShields, maxShields);
        }
    }

    #endregion

    #region IDamageable

    /* All colliders that can receive damage relays all their data to this script.
     * Logic is handled here, all other relevant scripts reference this in order to grab modifiers.
     */

    //
    public DamageEnd MainDamage(Damage damage, Transform damageSource)
    {
        // reset shield timer
        rechargeTime = rechargeDelay;

        // tell HUD rotation for damage indicator
        OnReceiveDamage?.Invoke(damageSource.position, damage.baseDamage * 1.5f / maxHealth);

        // process damage
        float processedDamage = damage.baseDamage * damage.shieldMulti;
        float outcomeDamage = Mathf.Clamp(processedDamage - currentShields, 0f, processedDamage);

        // shield damage
        if (outcomeDamage > 0f)
        {
            // get proper damage
            // turn left over damage into armor damage
            processedDamage = (outcomeDamage * damage.armorMulti) / damage.shieldMulti;

            // calculate any overpenetration or damage reduction
            float armorDifference = (float)(damage.penetrationLevel + armorWeakening - protectionLevel) * 0.25f;
            float penetrationDamage = processedDamage * Mathf.Clamp(armorDifference, 0f, 0.75f);
            processedDamage *= Mathf.Clamp(armorDifference + 1f, 0.25f, 1f);

            // get normal over-damage
            outcomeDamage = Mathf.Clamp(processedDamage - currentArmor, 0f, processedDamage);

            OnChangeShields?.Invoke(currentShields, maxShields);

            // armor damage
            if (outcomeDamage > 0f || penetrationDamage > 0f)
            {
                // rest shield timer
                healTime = healDelay;

                // damage armor
                if (processedDamage < currentArmor) currentArmor -= processedDamage;
                else currentArmor = 0f;
                OnChangeArmor?.Invoke(currentArmor, maxArmor, protectionLevel, armorWeakening);
                Debug.Log("armor damage: " + processedDamage);

                // get proper damage
                // turn left over damage into health damage
                processedDamage = (outcomeDamage + penetrationDamage) / damage.armorMulti;

                // add to autoheal
                amountHealed = Mathf.Clamp(amountHealed - processedDamage, 0f, maxSelfHeal);

                // health damage
                if (processedDamage > currentHealth || (canBeInstakilled && damage.isCrit && damage.canInstakill))
                {
                    currentHealth = 0f;
                    Debug.Log("health damage: " + processedDamage);

                    OnChangeHealth?.Invoke(currentHealth, maxHealth);
                    
                    return DamageEnd.kill;
                }
                else
                {
                    currentHealth -= processedDamage;
                    Debug.Log("health damage: " + processedDamage);

                    OnChangeHealth?.Invoke(currentHealth, maxHealth);

                    // return normal health hit or crit
                    if (damage.isCrit) return DamageEnd.crit;
                    else return DamageEnd.health;
                }
            }
            else
            {
                currentArmor -= processedDamage;
                Debug.Log("armor damage: " + processedDamage);
                OnChangeArmor?.Invoke(currentArmor, maxArmor, protectionLevel, armorWeakening);
                return DamageEnd.armor;
            }
        }
        else
        {
            currentShields -= processedDamage;
            Debug.Log("shield damage: " + processedDamage);
            OnChangeShields?.Invoke(currentShields, maxShields);
            return DamageEnd.shields;
        }
    }

    /*
    public HitpointType MainDamage(Damage damage)
    {
        // what needs to happen: reset hemorrhage ticks, deal damage to shields->armor->health
        // get damage values
        float individualDMG = 0f;
        int netPenetration;
        float armorDMG = 0f, bleedthrough = 0f;
        
        // shields first
        if (currentShields > 0f)
        {
            individualDMG = damage.baseDamage * damage.shieldMulti;

            if (individualDMG >= currentShields)
            {
                damage.baseDamage -= currentShields;
                currentShields = 0f;
                rechargeTime = rechargeDelay;
            }
            else
            {
                currentShields -= (individualDMG / damage.shieldMulti);
                rechargeTime = rechargeDelay;
                return HitpointType.shields;
            }
        }

        // armor second
        if (currentArmor > 0f)
        {
            netPenetration = damage.penetrationLevel - protectionLevel + armorWeakening;
            
            if (netPenetration <= 0)
            {
                armorDMG = damage.baseDamage * Mathf.Clamp((float)(Mathf.Abs(netPenetration) / 4),0.25f, 1f);                
            }
            else
            {
                armorDMG = damage.baseDamage;
                bleedthrough += damage.baseDamage * Mathf.Clamp((float)(4 - Mathf.Abs(netPenetration) / 4),0f, 1f);
            }

            individualDMG = armorDMG * damage.armorMulti;

            if (individualDMG >= currentArmor)
            {
                damage.baseDamage = Mathf.Clamp(damage.baseDamage - currentArmor + bleedthrough, 0f, damage.baseDamage);
                currentArmor = 0f;
            }
            else if (bleedthrough <= 0f)
            {
                currentArmor -= (individualDMG / damage.armorMulti);
                return HitpointType.armor;
            }
        }

        // health last
        damage.baseDamage *= damage.healthMulti;

        // don't forget crits!
        if (damage.isCrit)
        {
            if (critsInstakill)
            {
                damage.baseDamage *= Mathf.Pow(10f,30f);
            }
            else
            {
                damage.baseDamage *= damage.critMulti;
            }
        }

        if (damage.baseDamage >= currentHealth)
        {
            currentHealth = 0f;

            OnDeath();
            return HitpointType.health;
        }
        else
        {
            currentHealth -= damage.baseDamage;
            return HitpointType.health;
        }
    } */

    public void OffsetPoise(int stagger)
    {
        float newPoise = Mathf.Clamp(currentPoise + stagger, 0f, maxPoise);

        if (newPoise >= currentPoise)
        {
            recoveryTime = recoveryDelay;
        }

        currentPoise = newPoise;
    }

    public void OffsetRadiation(int radiation)
    {
        float newRadiation = Mathf.Clamp(currentRadiation + radiation, 0f, maxRadiation);

        if (newRadiation >= currentRadiation)
        {
            decayTime = decayDelay;
        }

        currentRadiation = newRadiation;

        if (currentHealth > (maxHealth - currentRadiation))
        {
            currentHealth = (maxHealth = currentRadiation);
        }
    }
    
    public void OffsetFrost(int frost)
    {
        float newFrost = Mathf.Clamp(currentFrost + frost, 0f, maxFrost);

        if (newFrost > currentFrost)
        {
            meltTime = meltDelay;
        }

        currentFrost = newFrost;
    }
    
    public void SetHemorrhage(Hemorrhage level)
    {
        switch (level)
        {
            case Hemorrhage.major:
                hemorrhageLevel = Hemorrhage.major;
                numTicksCurrent = 0;
                sutureTime = sutureTick;
                break;
            case Hemorrhage.minor:
                if (hemorrhageLevel != Hemorrhage.major)
                    hemorrhageLevel = Hemorrhage.minor;
                numTicksCurrent = 0;
                sutureTime = sutureTick;
                break;
            case Hemorrhage.none:
                numTicksCurrent = 0;
                sutureTime = sutureTick;
                break;
        }
    }

    public void SetShock(bool newShocked)
    {
        isShocked = newShocked;
    }
    
    public string GetActorID() { return thisActor.GetID(); }
    public float GetPoise() { return currentPoise; }
    public float GetRadiation() { return currentRadiation; }
    public float GetFrost() { return currentFrost; }
    public Hemorrhage GetHemorrhage() { return hemorrhageLevel; }
    public bool GetShocked() { return isShocked; }

    #endregion
}
