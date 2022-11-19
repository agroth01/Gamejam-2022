// /* --------------------
// -----------------------
// Creation date: 15/11/2022
// Author: Alex
// Description: This enemy will try to snipe the player when in line of sight. This is different from other enemies,
//              as the player
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperEnemy : Enemy
{
    [Header("Snipe")]
    [SerializeField] private int m_recharge;
    [SerializeField] private int m_maxRange;
    [SerializeField] private int m_damage;

    [Header("Audio")]
    [SerializeField] private AudioClip m_lockOnSound;
    [SerializeField] private AudioClip m_shootSound;

    public override void DetermineAction()
    {
        // Ignore unless within range
        if (Grid.Instance.GetDistanceBetweenUnits(this, GetPlayer()) > m_maxRange)
        {
            return;
        }

        // Make sure there is line of sight to the player.
        if (!LineOfSightToPlayer())
        {
            SetLine("no los");
            return;
        }

        // Create the action
        ICombatAction targetedShot = new TargetedShotAction(this, m_damage, () => { PlaySound(m_shootSound); });
        SetAction(targetedShot);
        SetLine("snipe", m_damage);
        PlaySound(m_lockOnSound);
    }

    public override void DetermineMove()
    {
        
    }
}
