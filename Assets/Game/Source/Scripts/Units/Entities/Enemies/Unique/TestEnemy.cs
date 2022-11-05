using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : EnemyBase
{
    public override void Awake()
    {
        base.Awake();

        m_health.OnHealthZero += Die;
    }

    public override void TakeDamage(int damage)
    {
        m_health.Damage(damage);        
    }

    /// <summary>
    /// Called when enemy dies.
    /// </summary>
    private void Die()
    {
        Destroy(gameObject);
    }
}
