// /* --------------------
// -----------------------
// Creation date: 05/11/2022
// Author: Alex
// Description: A object that can be damaged.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableObjectUnit : Unit
{
    [Header("Generic")]
    [SerializeField] private int m_maxHealth;
    [SerializeField] private int m_startingHealth;

    private Health m_health;

    public override void Awake()
    {
        base.Awake();
        InitializeHealth();
    }

    public void InitializeHealth()
    {
        m_health = new Health(m_maxHealth, m_startingHealth);
        m_health.OnHealthZero += OnDeath;
    }

    public override void TakeDamage(int damage)
    {
        m_health.Damage(damage);
    }

    public void OnDeath()
    {
        RemoveUnit();
    }
}
