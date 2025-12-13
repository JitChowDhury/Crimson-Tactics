using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour, AI
{
    [SerializeField] private ObstacleData obstacleData; // shared obstacle grid
    [SerializeField] private Animator animator;         // enemy animations ( I havent added any for now same as playermovement)

    // current grid position
    private int currentX;
    private int currentZ;

    // movement lock
    private bool isMoving;

    // register this Ai with the central AI manager
    private void OnEnable()
    {
        FindFirstObjectByType<AIManager>()?.Register(this);
    }

    // unregister when disabled
    private void OnDisable()
    {
        FindFirstObjectByType<AIManager>()?.Unregister(this);
    }

    void Start()
    {
        // initialize grid position from world position
        currentX = Mathf.RoundToInt(transform.position.x);
        currentZ = Mathf.RoundToInt(transform.position.z);

        // auto correct spawn if enemy starts on obstacle
        if (IsObstacle(currentX, currentZ))
        {
            Vector2Int safe = FindNearestSafeTile(currentX, currentZ);
            transform.position = new Vector3(safe.x, 0.9f, safe.y);
            currentX = safe.x;
            currentZ = safe.y;
        }
    }

    // called by AIManager after player finishes moving
    public void OnPlayerMoved(Vector2Int playerGridPos)
    {
        // do not interrupt ongoing movement
        if (isMoving) return;

        // choose one valid adjacent tile near the player
        Vector2Int target = GetAdjacentTile(playerGridPos);
        if (target == Vector2Int.zero) return;

        // forbid enemy from stepping onto player's tile
        List<Vector2Int> forbidden = new List<Vector2Int>();
        forbidden.Add(playerGridPos);

        // request path from shared pathfinding system
        List<Vector3> path = PathfindingManager.FindPath(
            new Vector2Int(currentX, currentZ),
            target,
            obstacleData,
            forbidden
        );

        // move only if path is valid
        if (path != null && path.Count > 0)
            StartCoroutine(MoveAlongPath(path));
    }

    // selects the first valid adjacent tile around the player
    Vector2Int GetAdjacentTile(Vector2Int playerPos)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var d in dirs)
        {
            Vector2Int adj = playerPos + d;
            if (IsInsideGrid(adj.x, adj.y) && !IsObstacle(adj.x, adj.y))
                return adj;
        }

        return Vector2Int.zero;
    }

    // grid bounds check
    bool IsInsideGrid(int x, int z)
    {
        return x >= 0 && x < 10 && z >= 0 && z < 10;
    }

    // obstacle lookup
    bool IsObstacle(int x, int z)
    {
        return obstacleData.obstacles[x].row[z];
    }

    // finds nearest non obstacle tile around spawn
    Vector2Int FindNearestSafeTile(int x, int z)
    {
        for (int radius = 1; radius < 10; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    int nx = x + dx;
                    int nz = z + dz;

                    if (IsInsideGrid(nx, nz) && !IsObstacle(nx, nz))
                        return new Vector2Int(nx, nz);
                }
            }
        }

        return new Vector2Int(0, 0); // fallback
    }

    // moves enemy step by step along computed path
    IEnumerator MoveAlongPath(List<Vector3> path)
    {
        isMoving = true;

        //lock global movement while enemy moves
        AIManager.SetMovementState(true);

        if (animator) animator.SetBool("IsWalking", true);

        foreach (var step in path)
        {
            Vector3 target = new Vector3(step.x, 0.9f, step.z);
            Vector3 dir = (target - transform.position).normalized;

            //rotate towards movement direction
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            //smooth movement to next tile
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, target, Time.deltaTime * 3f);
                yield return null;
            }

            // update grid position
            currentX = Mathf.RoundToInt(step.x);
            currentZ = Mathf.RoundToInt(step.z);
        }

        // stop animation and unlock movement
        if (animator) animator.SetBool("IsWalking", false);
        isMoving = false;
        AIManager.SetMovementState(false);
    }
}
