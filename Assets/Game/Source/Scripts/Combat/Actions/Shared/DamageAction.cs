// /* --------------------
// -----------------------
// Creation date: 05/11/2022
// Author: Alex
// Description: This action will deal damage to something that has IDamagable interface.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAction : ICombatAction
{
    private IDamagable m_target;
    private int m_damageAmount;

    public void Execute()
    {
        m_target.TakeDamage(m_damageAmount);
    }

    public DamageAction(IDamagable target, int damageAmount)
    {
        m_target = target;
        m_damageAmount = damageAmount;
    }
}
