// /* --------------------
// -----------------------
// Creation date: 14/11/2022
// Author: Alex
// Description: This action will move the unit towards the given position, and stop at first obstacle.
//              If possible, the thing that stopped the charge will take damage and be knocked back.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAction : ICombatAction
{
    private Unit m_unit;
    private Direction m_direction;
    private int m_range;
    private int m_damage;
    private int m_knockbackForce;
    private int m_speed;

    public IEnumerator Execute()
    {
        Vector2Int chargePosition = m_unit.GridPosition;
        Vector2Int nextPosition = Grid.Instance.PositionWithDirection(chargePosition, m_direction);

        bool stopped = false;
        for (int x = 0; x < m_range; x++)
        {
            // Is the next position blocked?
            if (!Grid.Instance.IsTileFree(nextPosition))
            {
                stopped = true;
                break;
            }
            
            chargePosition = nextPosition;

            // Move towards the new position
            ICombatAction move = new MoveAction(m_unit as Entity, chargePosition, (float)m_speed);
            yield return move.Execute();
         
            nextPosition = Grid.Instance.PositionWithDirection(chargePosition, m_direction);
        }

        if (stopped)
        {
            // Check if there is a target at the position.
            Unit unit = Grid.Instance.GetUnitAt(nextPosition);
            if (unit != null)
            {
                unit.TakeDamage(m_damage);
                unit.Push(m_direction, m_knockbackForce);
            }
        }
    }

    public ChargeAction(Unit unit, Vector2Int targetPosition, int range, int damage, int knockbackForce, int speed)
    {
        m_unit = unit;
        m_direction = Grid.Instance.GetDirectionTo(targetPosition, unit.GridPosition);
        m_range = range;
        m_damage = damage;
        m_knockbackForce = knockbackForce;
        m_speed = speed;
    }
}
