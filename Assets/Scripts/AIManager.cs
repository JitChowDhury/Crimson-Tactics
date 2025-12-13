using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static bool IsAnyUnitMoving = false;//prevent enemy and playe move at same time

    private List<AI> aiAgents = new List<AI>();//stores all AI units
    //register an agent when it becomes active in scene
    public void Register(AI agent)
    {
        if (!aiAgents.Contains(agent))
            aiAgents.Add(agent);
    }
    //Unregister an agent when it becomes active in scene
    public void Unregister(AI agent)
    {
        if (aiAgents.Contains(agent))
            aiAgents.Remove(agent);
    }
    //called when player finished moving
    public void NotifyPlayerMoved(Vector2Int playerGridPos)
    {
        foreach (var agent in aiAgents)
        {
            agent.OnPlayerMoved(playerGridPos);
        }
    }
    //Used by PlayerMovement and EnemyAI to lock input and movement
    public static void SetMovementState(bool state)
    {
        IsAnyUnitMoving = state;
    }


}
