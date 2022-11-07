// /* --------------------
// -----------------------
// Creation date: 05/11/2022
// Author: Alex
// Description: This action will deal damage to anything at a single tile.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleDamageAction : ICombatAction
{
    private Vector2Int m_targetPosition;
    private int m_damageAmount;

    public void Execute()
    {
        // Check if there is a target at the position.
        IDamagable target = Grid.Instance.GetUnitAt(m_targetPosition);
        if (target != null)
        {
            target.TakeDamage(m_damageAmount);
        }
    }

    public SingleDamageAction(Vector2Int targetPosition, int damageAmount)
    {
        m_targetPosition = targetPosition;
        m_damageAmount = damageAmount;
    }
}
