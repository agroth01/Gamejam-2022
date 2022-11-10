using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatActionManager : MonoBehaviour
{
    // Singleton manager
    public static CombatActionManager Instance { get; private set; }

    // We use a stack instead of a list, since we want to be able to undo actions.
    // Stacks are always LIFO, so we can just pop the last action and undo it.
    private Stack<ICombatAction> m_combatActionBuffer = new Stack<ICombatAction>();

    private void Awake()
    {
        // Initialize singleton and yell at teammate if more than one instance exists.
        if (Instance == null) Instance = this;
        else Debug.LogError("Theo you messed up the Singleton.");
    }

    public void AddCombatAction(ICombatAction combatAction)
    {
        combatAction.Execute();
        m_combatActionBuffer.Push(combatAction);
    }
}
