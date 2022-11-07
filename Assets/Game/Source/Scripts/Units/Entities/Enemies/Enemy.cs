// /* --------------------
// -----------------------
// Creation date: 05/11/2022
// Author: Alex
// Description: The base for any enemy in the game. For simplicty sake since we don't have time for very complicated
//              enemy setup, each enemy will have it's own class that inherits from this one.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity, IPushable
{
    [Header("Generic")]
    [SerializeField] private int m_maxHealth;
    [SerializeField] private int m_startingHealth;
    [SerializeField] private int m_speed;

    // Collecion of possible attacks.

    public override void InitializeHealth()
    {
        m_health = new Health(m_maxHealth, m_startingHealth);
        m_health.OnHealthZero += OnDeath;
    }

    public override void TakeDamage(int damage)
    {
        m_health.Damage(damage);
    }

    /// <summary>
    /// The method that will get called when deciding on what action should be taken by the enemy.
    /// Should be overridden by each enemy type to decide what actions to use.
    /// </summary>
    public virtual void DetermineAction() { }

    /// <summary>
    /// Sends the action that the enemy will perform to the queue to be executed.
    /// </summary>
    /// <param name="action"></param>
    public void SetAction(ICombatAction action)
    {
        BattleManager.Instance.AddActionToQueue(action, m_speed);
    }

    /// <summary>
    /// Removes the action from the queue.
    /// </summary>
    /// <param name="action"></param>
    public void RemoveAction(ICombatAction action)
    {
        BattleManager.Instance.RemoveActionFromQueue(action);
    }

    public override void OnDeath()
    {
        RemoveUnit();
    }
}
