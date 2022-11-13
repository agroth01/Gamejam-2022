// /* --------------------
// -----------------------
// Creation date: 10/11/2022
// Author: Alex
// Description: A ranged enemy that will attack from a distance.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Ranged attack")]
    [SerializeField] private int m_range;
    [SerializeField] private Hazards m_hazardType;
    [SerializeField] private int m_hazardDuration;
    [SerializeField] private int m_hazardApplicationDuration;
    [SerializeField] private int m_hazardDamage;

    public override void DetermineAction()
    {
        // First determine if the enemy is in range of the player. Skip action otherwise.
        if (Grid.Instance.GetDistance(GridPosition, GetPlayer().GridPosition) > m_range)
            return;

        // Get all tiles around the player
        List<Vector2Int> targetTiles = Grid.Instance.GetSurroundingTiles(GetPlayer().GridPosition, 1);
        targetTiles.Add(GetPlayer().GridPosition);

        // Now we have to create the hazard
        ICombatAction hazardCreation = new CreateHazardAction(targetTiles, m_hazardType, m_hazardDamage, m_hazardDuration, m_hazardApplicationDuration);
        SetAction(hazardCreation);
    }

    public override void DetermineMove()
    {
        
    }
}
