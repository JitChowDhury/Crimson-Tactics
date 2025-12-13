using UnityEngine;

public interface AI
{
    // called when the player finishes a move and AI should react
    void OnPlayerMoved(Vector2Int playerGridPos);
}
