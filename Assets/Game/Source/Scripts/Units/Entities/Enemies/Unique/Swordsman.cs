// /* --------------------
// -----------------------
// Creation date: 06/11/2022
// Author: Alex
// Description: This is a very common type of enemy that will only attack with melee attacks.
// -----------------------
// ------------------- */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swordsman : Enemy
{
    [Header("Swordsman")]
    [SerializeField] private int m_damage;

    public override void DetermineAction()
    {
        // Create attack in random direction
        Direction direction = (Direction)typeof(Direction).GetRandomValue();
        Vector2Int attackPosition = Grid.Instance.PositionWithDirection(GridPosition, direction);

        Debug.Log(gameObject.name + " intending to attack: " + Enum.GetName(typeof(Direction), direction));

        ICombatAction attack = new DamageAction(attackPosition, m_damage);
        SetAction(attack);
    }
}
