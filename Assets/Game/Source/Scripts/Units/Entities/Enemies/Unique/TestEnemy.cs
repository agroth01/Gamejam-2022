using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : Enemy
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void TakeDamage(int damage)
    {
        m_health.Damage(damage);        
    }
}
