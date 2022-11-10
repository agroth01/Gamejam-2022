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

public abstract class Enemy : Entity, IPushable
{
    [Header("Generic")]
    [SerializeField] private string m_displayName;
    [SerializeField] private int m_actionPriority;

    [Header("Highlights")]
    [SerializeField] private Color m_damageHighlightColor;
    [SerializeField] private Color m_moveHighlightColor;
    private List<GameObject> m_highlights;

    // Track the intended action, for automatic removal upon death
    private ICombatAction m_intendedAction;

    public int Prority
    {
        get { return m_actionPriority; }
    }

    public override void InitializeHealth()
    {
        m_health = new Health(m_maxHealth, m_startingHealth);
        m_health.OnHealthZero += OnDeath;
    }

    public override void TakeDamage(int damage)
    {
        m_health.Damage(damage);
    }

    /// <summary>
    /// The method that will get called when deciding on what action should be taken by the enemy.
    /// Should be overridden by each enemy type to decide what actions to use.
    /// </summary>
    public abstract void DetermineAction();
    public abstract void DetermineMove();

    public void OnMouseEnter()
    {
        if (m_highlights != null)
            ShowHighlights();
    }

    private void OnMouseExit()
    {
        if (m_highlights != null)
            HideHighlights();
    }


    /// <summary>
    /// Sends the action that the enemy will perform to the queue to be executed.
    /// </summary>
    /// <param name="action"></param>
    public void SetAction(ICombatAction action)
    {
        // Add it to the queue for when it's enemies turn to attack.
        BattleManager.Instance.AddActionToQueue(action, m_actionPriority);
        m_intendedAction = action;

        // Set up highlights for the action
        ClearHighlights();
        m_highlights = CreateHighlights(action);
        HideHighlights();
    }

    /// <summary>
    /// Removes the action from the queue.
    /// </summary>
    /// <param name="action"></param>
    public void RemoveAction(ICombatAction action)
    {
        BattleManager.Instance.RemoveActionFromQueue(action);
        ClearHighlights();
    }

    public override void OnDeath()
    {
        // Note that we do not need to clear the highlights here when the enemy dies,
        // because it is handled when removing action.
        RemoveAction(m_intendedAction);
        RemoveUnit();
    }

    #region Player

    /// <summary>
    /// Gets the direction from this enemy to the player.
    /// </summary>
    /// <returns>Direction to player</returns>
    public Direction GetDirectionToPlayer()
    {
        Vector2Int currentPosition = Grid.Instance.GetGridPosition(transform.position);
        return Grid.Instance.GetDirectionTo(currentPosition, GetPlayer().GridPosition);
    }

    /// <summary>
    /// Gets a reference to the player on the grid.
    /// </summary>
    /// <returns></returns>
    public Player GetPlayer()
    {
        // There will always just be one player on the board,
        // so we can reliably use index 0.
        return (Player)Grid.Instance.GetUnitsOfType<Player>()[0];
    }

    #endregion

    #region Highlights

    /// <summary>
    /// Creates highlights based on the type of action is performed
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    private List<GameObject> CreateHighlights(ICombatAction action)
    {
        List<GameObject> highlights = new List<GameObject>();

        // Single target damage
        if (action is SingleDamageAction)
        {
            SingleDamageAction singleDamageAction = (SingleDamageAction)action;
            highlights.Add(Grid.Instance.HighlightTile(singleDamageAction.TargetPosition, m_damageHighlightColor));
        }

        // Area Damage Action
        if (action is AreaDamageAction)
        {
            AreaDamageAction areaDamageAction = (AreaDamageAction)action;
            foreach (SingleDamageAction sda in areaDamageAction.Actions)
            {
                highlights.Add(Grid.Instance.HighlightTile(sda.TargetPosition, m_damageHighlightColor));
            }
        }

        // Normal move
        if (action is MoveAction)
        {
            MoveAction moveAction = (MoveAction)action;
            foreach (Vector2Int position in moveAction.Destinations)
            {
                highlights.Add(Grid.Instance.HighlightTile(position, m_moveHighlightColor));
            }
        }

        return highlights;
    }

    /// <summary>
    /// Destroys all highlight objects
    /// </summary>
    private void ClearHighlights()
    {
        if (m_highlights == null) return;

        foreach (GameObject highlight in m_highlights)
        {
            Destroy(highlight);
        }
        m_highlights = null;
    }

    /// <summary>
    /// Disables all the highlight objects
    /// </summary>
    private void HideHighlights()
    {
        foreach (GameObject highlight in m_highlights)
        {
            highlight.SetActive(false);
        }
    }

    /// <summary>
    /// Enables all the highlight objects
    /// </summary>
    private void ShowHighlights()
    {
        foreach (GameObject highlight in m_highlights)
        {
            highlight.SetActive(true);
        }
    }

    #endregion
}
