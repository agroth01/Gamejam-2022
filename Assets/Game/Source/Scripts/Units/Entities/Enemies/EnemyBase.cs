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

public class EnemyBase : Entity, IPushable
{
    [Header("Generic")]
    [SerializeField] private int m_maxHealth;
    [SerializeField] private int m_startingHealth;

    // Collecion of possible attacks.

    public override void InitializeHealth()
    {
        m_health = new Health(m_maxHealth, m_startingHealth);
    }

    public override void OnDeath()
    {
        Destroy(gameObject);
    }
}
