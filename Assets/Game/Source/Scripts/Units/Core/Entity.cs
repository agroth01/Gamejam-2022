// /* --------------------
// -----------------------
// Creation date: 04/11/2022
// Author: Alex
// Description: An entity is anything that is considered "alive" in the game. So, this is for player and enemy units.
//              By default, all entities are pushable and damagable, have health and methods for moving around on the grid.
//
//              Note: While all entities will have a health instance, it will not be instanciated by default. This is because
//              entities like the enemies will set up their health parameters a different way than say the player.
// -----------------------
// ------------------- */

using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : Unit, IPushable
{
    // Learning note:
    // Protected keyword is used for variables that are only accessible by the class itself and any inheriting classes.
    protected Health m_health;

    public override void Awake()
    {
        base.Awake();
        InitializeHealth();
    }

    public abstract void InitializeHealth();
    public abstract void OnDeath();

    public void Push(Vector2Int pushForce)
    {
        
    }
}
