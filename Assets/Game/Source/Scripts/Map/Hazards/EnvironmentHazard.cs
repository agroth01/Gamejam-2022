// /* --------------------
// -----------------------
// Creation date: 11/11/2022
// Author: Alex
// Description: A hazard that will be placed on the map and will apply a debuff to any entity that passes
//              through it or stops on it.
// -----------------------
// ------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{
    [Header("Hazard")]
    [SerializeField] private Hazards m_hazardType;
    [SerializeField] private int m_applicationDuration;
    [SerializeField] private int m_damage;

    public Hazards HazardType
    {
        get { return m_hazardType; }
    }

    private void Start()
    {
        // We need to register this hazard to the grid, so that it is visible to
        // other units.
        Vector2Int position = Grid.Instance.GetGridPosition(transform.position);
        Grid.Instance.RegisterHazard(this, position);
    }

    private void OnDisable()
    {
        // Unregister from grid, as if disabled or removed, we dont want the hazard to be visible or applicable
        // to any units.
        Grid.Instance.UnregisterHazard(this);
    }

    /// <summary>
    /// Applies the status effect to the specified unit.
    /// </summary>
    /// <param name="unit">The unit to apply effect to.</param>
    public void ApplyHazard(Unit unit)
    {
        // Create the new status effect and add it to the unit.
        StatusEffect effect = GetEffect();
        unit.AddStatusEffect(effect);
    }

    /// <summary>
    /// Determines what effect should be applied based on hazard type.
    /// </summary>
    /// <returns></returns>
    private StatusEffect GetEffect()
    {
        StatusEffect effect = null;

        switch(m_hazardType)
        {
            case Hazards.Fire:
                effect = new FireStatusEffect(m_damage, m_applicationDuration);
                break;

            case Hazards.Poison:
                effect = new PoisonStatusEffect(m_damage, m_applicationDuration);
                break;
        }

        return effect;
    }

}
