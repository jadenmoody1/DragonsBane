using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class Skill
{
    public string skillName;
    public int level = 1;
    public float currentXP = 0f;
    public float xpToNextLevel = 100f;
    public float xpMultiplier = 1.1f;

    [HideInInspector] public bool leveledUp = false;
    public UnityEvent<int, float, float> OnSkillChanged; // level, currentXP, xpToNextLevel

    public void AddXP(float amount)
    {
        if (amount <= 0) return;

        currentXP += amount;
        leveledUp = false;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            level++;
            xpToNextLevel *= xpMultiplier;
            leveledUp = true;
        }

        OnSkillChanged?.Invoke(level, currentXP, xpToNextLevel);
    }
}

public class RPGStats : MonoBehaviour
{
    [Header("Movement Stats")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpHeight = 2f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    [HideInInspector] public float currentStamina;
    public float staminaDrainRate = 10f;
    public float staminaRecoveryRate = 5f;
    public float jumpStaminaCost = 15f;
    public float staminaRecoveryDelay = 2f;

    private float staminaCooldownTimer = 0f;

    [Header("Health")]
    public float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    [Header("Skills")]
    public Skill[] skills;

    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged; 
    public UnityEvent<float, float> OnStaminaChanged; 

    void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // Auto-populate default skills if empty
        if (skills == null || skills.Length == 0)
        {
            skills = new Skill[]
            {
                new Skill() { skillName = "HitPoints" },
                new Skill() { skillName = "Athletics" },
                new Skill() { skillName = "Acrobatics" }
            };
        }

        // Initialize skill events
        foreach (var skill in skills)
        {
            if (skill.OnSkillChanged == null)
                skill.OnSkillChanged = new UnityEvent<int, float, float>();
        }
    }

    #region Health & Stamina
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void UseStamina(float amount)
    {
        if (amount <= 0) return;

        currentStamina = Mathf.Clamp(currentStamina - amount, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        staminaCooldownTimer = staminaRecoveryDelay;
    }

    public void RestoreStamina(float amount)
    {
        if (staminaCooldownTimer > 0f) return;
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
    #endregion

    #region Skills
    public void GainSkillXP(string skillName, float amount)
    {
        foreach (var skill in skills)
        {
            if (skill.skillName == skillName)
            {
                skill.AddXP(amount);
                // Apply skill effects
                if (skill.skillName == "Athletics" && skill.leveledUp)
                {
                    runSpeed += 0.2f;
                    walkSpeed += 0.1f;
                    maxStamina += 5f;
                }
                if (skill.skillName == "Acrobatics" && skill.leveledUp)
                {
                    jumpHeight += 0.2f;
                    maxStamina += 5f;
                }
                break;
            }
        }
    }

    public Skill GetSkill(string skillName)
    {
        foreach (var skill in skills)
            if (skill.skillName == skillName) return skill;
        return null;
    }
    #endregion

    #region Movement Helpers
    public float GetCurrentMoveSpeed(bool isRunning)
    {
        if (isRunning && currentStamina > 0f)
            return runSpeed;
        return walkSpeed;
    }

    public void HandleRunning(bool isRunning, float deltaTime)
    {
        if (staminaCooldownTimer > 0f)
            staminaCooldownTimer -= deltaTime;

        if (isRunning && currentStamina > 0f)
        {
            UseStamina(staminaDrainRate * deltaTime);
            GainSkillXP("Athletics", deltaTime);
        }
        else
        {
            RestoreStamina(staminaRecoveryRate * deltaTime);
        }
    }

    public void HandleJumping(float deltaTime)
    {
        GainSkillXP("Acrobatics", deltaTime);
    }
    #endregion
}
