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
    [SerializeField] private bool m_permanent;
    [SerializeField] private int m_duration;

    [Header("Status effect")]
    [SerializeField] private Hazards m_hazardType;
    [SerializeField] private int m_applicationDuration;
    [SerializeField] private int m_damage;

    private GameObject m_particles;
    private int m_roundsAlive;

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

        m_roundsAlive = 0;

        // Only for when the hazard is first created, check if there is a unit on the tile
        // and apply effect if there is.
        if (Grid.Instance.GetUnitAt(position) != null)
        {
            ApplyHazard(Grid.Instance.GetUnitAt(position));
        }
    }

    private void OnDisable()
    {
        // Unregister from grid, as if disabled or removed, we dont want the hazard to be visible or applicable
        // to any units.
        Grid.Instance.UnregisterHazard(this);
    }

    /// <summary>
    /// Creates a new hazard at the given position in the world.
    /// </summary>
    /// <param name="hazardType">Type of hazard to create</param>
    /// <param name="duration">How long the hazard should last</param>
    /// <param name="applicationDuration">How long status effect should be applied</param>
    /// <param name="damage">Damage of status effect</param>
    /// <param name="position">Position to spawn hazard</param>
    public static void CreateHazard(Hazards hazardType, int duration, int applicationDuration, int damage, Vector2Int position)
    {
        // Get the prefab for the hazard type.
        GameObject hazardPrefab = Resources.Load<GameObject>("Hazards/Hazard");
        if (hazardPrefab == null)
        {
            Debug.LogError("Could not find hazard prefab for hazard type: " + hazardType.ToString());
            return;
        }

        // Instantiate the hazard prefab.
        GameObject hazard = Instantiate(hazardPrefab, Grid.Instance.GetWorldPosition(position.x, position.y), Quaternion.identity);
        hazard.transform.parent = GameObject.Find("Hazards").transform;

        // Set the hazard's properties.
        EnvironmentHazard hazardScript = hazard.GetComponent<EnvironmentHazard>();
        hazardScript.SetupHazard(hazardType, applicationDuration, damage, duration);
    }

    /// <summary>
    /// Sets the variables of the hazard
    /// </summary>
    /// <param name="hazardType">The type of hazard it is</param>
    /// <param name="applicationDuration">How long the hazard stays on grid.</param>
    /// <param name="damage">How much damage the hazard should do.</param>
    public void SetupHazard(Hazards hazardType, int applicationDuration, int damage, int duration)
    {
        m_hazardType = hazardType;
        m_applicationDuration = applicationDuration;
        m_damage = damage;
        m_duration = duration;

        CreateParticles();
    }

    /// <summary>
    /// Updates the duration of the hazard if not permanent and will remove it if the duration is 0.
    /// Will be called at the start of each round.
    /// </summary>
    public void UpdateHazard()
    {
        // Ignore if the hazard is permanent.
        if (m_permanent) return;

        m_roundsAlive += 1;
        if (m_roundsAlive == m_duration)
            RemoveHazard();
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
    /// Removes the hazard from the grid. Should be used instead of destroying gameobject
    /// directly.
    /// </summary>
    public void RemoveHazard()
    {
        Grid.Instance.UnregisterHazard(this);

        // Instead of instantly destroying, we first stop the particles from looping,
        // then destroy after a small delay. This gives the removal a more "natural" stop
        // than all particles instantly disappearing.
        ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
        if (particles != null)
        {
            particles.Stop();

            // Destroy after particles lifetime.
            Destroy(gameObject, particles.main.startLifetime.constant);
        }
        else
        {
            Destroy(gameObject);
        }
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

    /// <summary>
    /// Instantiates particles as a child of this from resources based on the type of hazard.
    /// </summary>
    private void CreateParticles()
    {
        string particlePath = "Hazards/Particles/" + m_hazardType.ToString() + "Particles";
        GameObject particles = Instantiate(Resources.Load(particlePath), transform) as GameObject;
        m_particles = particles;
    }
}
