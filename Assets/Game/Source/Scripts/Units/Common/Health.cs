// /* --------------------
// -----------------------
// Creation date: 04/11/2022
// Author: Alex
// Description: Simple health class with callbacks on various events.
//              We separate this from the entity class because we might want to have units that are not damagable.
// -----------------------
// ------------------- */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health
{
    #region Public Properties

    /// <summary>
    /// The current amount of health left.
    /// </summary>
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Maximum amount of health possible.
    /// </summary>
    public int MaxHealth { get; private set; }

    // We define various events for the health class.
    public Action OnHealthChanged; // TODO: Maybe pass amount health changed by? So we can react to stuff like big hits.
    public Action OnHealthZero;
    public Action OnDamaged;
    public Action OnHealed;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new health instance with only max health.
    /// Current health will be set to max.
    /// </summary>
    /// <param name="maxHealth">The max health possible.</param>
    public Health(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth;
    }

    /// <summary>
    /// Creates a new health instance with custom starting health.
    /// </summary>
    /// <param name="maxHealth">Max health possible.</param>
    /// <param name="currentHealth">Current health on creation.</param>
    public Health(int maxHealth, int currentHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Removes health.
    /// </summary>
    /// <param name="amount">The amount to remove.</param>
    public void Damage(int amount)
    {
        ModifyHealth(CurrentHealth - amount);
        OnDamaged?.Invoke();
    }

    /// <summary>
    /// Adds health.
    /// </summary>
    /// <param name="amount">The amount to add</param>
    public void Heal(int amount)
    {
        ModifyHealth(CurrentHealth + amount);
        OnHealed?.Invoke();
    }

    /// <summary>
    /// Instantly sets the health value to 0.
    /// </summary>
    public void Kill()
    {
        ModifyHealth(0);
    }

    /// <summary>
    /// Sets the health back to max health.
    /// </summary>
    public void Restore()
    {
        ModifyHealth(MaxHealth);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Sets the value of the current health directly.
    /// </summary>
    /// <param name="newValue">The new value of current health.</param>
    private void ModifyHealth(int newValue)
    {
        // The current health needs to be clamped between 0 and maxhealth.
        int oldHealth = CurrentHealth;
        CurrentHealth = newValue;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        // OnHealthChanged should be called after the actual value is modified, so that elements
        // relying on the health value can get the correct value.
        if (newValue != oldHealth) OnHealthChanged?.Invoke();

        // Call event in case we want to do something when the health reaches 0.
        if (CurrentHealth == 0) OnHealthZero?.Invoke();
    }

    #endregion
}
