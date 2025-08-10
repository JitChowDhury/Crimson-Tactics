using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour, AI
{
    [SerializeField] private ObstacleData obstacleData;//reference to the obstacle data
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;
    //enemy's current grid position
    private int currentX;
    private int currentZ;
    private bool isMoving = false;

    //stores the path the enemy will follow
    private List<Vector3> path = new List<Vector3>();

    //subscribe and unsubscribe to events
    private void OnEnable()
    {
        PlayerMovement.OnPlayerMoveComplete += HandlePlayerMoveComplete;
    }

    private void OnDisable()
    {
        PlayerMovement.OnPlayerMoveComplete -= HandlePlayerMoveComplete;
    }

    void Start()
    {
        //current positiom
        currentX = Mathf.RoundToInt(transform.position.x);
        currentZ = Mathf.RoundToInt(transform.position.z);
    }

    public void UpdateAI()
    {

    }

    private void HandlePlayerMoveComplete()
    {
        if (isMoving) return;//if player moving then return

        //get player pos
        Vector2Int playerPos = new Vector2Int(
            Mathf.RoundToInt(playerTransform.position.x),
            Mathf.RoundToInt(playerTransform.position.z)
        );

        Vector2Int targetAdjacent = GetAdjacentToPlayer(playerPos);

        if (targetAdjacent != Vector2Int.zero)
        {
            path = FindPath(currentX, currentZ, targetAdjacent.x, targetAdjacent.y);
            if (path != null && path.Count > 0)
            {
                StartCoroutine(MoveAlongPath(path));
            }
        }
    }
    //return adjacnet grid
    Vector2Int GetAdjacentToPlayer(Vector2Int playerPos)
    {
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int adjacent = playerPos + dir;
            if (IsInsideGrid(adjacent.x, adjacent.y) && !IsObstacle(adjacent.x, adjacent.y))
            {
                return adjacent;
            }
        }
        return Vector2Int.zero;
    }
    //check inside grid or a obstacle
    bool IsInsideGrid(int x, int z)
    {
        return x >= 0 && x < 10 && z >= 0 && z < 10;
    }

    bool IsObstacle(int x, int z)
    {
        if (obstacleData == null) return false;
        return obstacleData.obstacles[x].row[z]; // fixed index for 2D array
    }
    //find path using bfs algo
    List<Vector3> FindPath(int startX, int startZ, int targetX, int targetZ)
    {
        if (IsObstacle(targetX, targetZ)) return null;

        Queue<Vector3> queue = new Queue<Vector3>();
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        Vector3 start = new Vector3(startX, 0, startZ);
        Vector3 target = new Vector3(targetX, 0, targetZ);

        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector3[] directions = {
            new Vector3(1,0,0), new Vector3(-1,0,0),
            new Vector3(0,0,1), new Vector3(0,0,-1)
        };

        while (queue.Count > 0)
        {
            Vector3 current = queue.Dequeue();
            if (current == target) break;

            foreach (var dir in directions)
            {
                Vector3 neighbor = current + dir;
                int nx = (int)neighbor.x;
                int nz = (int)neighbor.z;

                if (IsInsideGrid(nx, nz) && !IsObstacle(nx, nz) && !cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(target)) return null;

        List<Vector3> finalPath = new List<Vector3>();
        Vector3 currentPathTile = target;

        while (currentPathTile != start)
        {
            finalPath.Insert(0, currentPathTile);
            currentPathTile = cameFrom[currentPathTile];
        }

        return finalPath;
    }
    //move along the path returned by the algo
    IEnumerator MoveAlongPath(List<Vector3> path)
    {
        isMoving = true;
        if (animator) animator.SetBool("IsWalking", true);

        foreach (var step in path)
        {
            Vector3 targetPos = new Vector3(step.x, 0.9f, step.z);
            Vector3 direction = (targetPos - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = lookRotation;
            }

            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 5f);
                yield return null;
            }

            currentX = (int)step.x;
            currentZ = (int)step.z;
        }

        if (animator) animator.SetBool("IsWalking", false);
        isMoving = false;
    }
}
