using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Fired when player finishes moving 
    public static System.Action<Vector2Int> OnPlayerMoveComplete;

    [SerializeField] private ObstacleData obstacleData; // grid obstacle data
    [SerializeField] private Animator animator;         // player animations
    [SerializeField] private Transform enemyTransform;  // enemy reference
    [SerializeField] private AudioSource click;         // click sound

    // current grid position
    private int currentX;
    private int currentZ;

    // movement lock
    private bool isMoving;

    void Start()
    {
        // initialize grid position from world position
        currentX = Mathf.RoundToInt(transform.position.x);
        currentZ = Mathf.RoundToInt(transform.position.z);

        // auto correct spawn if player starts on obstacle
        if (obstacleData.obstacles[currentX].row[currentZ])
        {
            Vector2Int safe = FindNearestSafeTile(currentX, currentZ);
            transform.position = new Vector3(safe.x, 0.9f, safe.y);
            currentX = safe.x;
            currentZ = safe.y;
        }
    }

    void Update()
    {
        // block input while any unit is moving
        if (isMoving || AIManager.IsAnyUnitMoving) return;

        // left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            click.Play();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // must hit something
            if (!Physics.Raycast(ray, out hit)) return;

            // convert world position to grid position
            int targetX = Mathf.RoundToInt(hit.point.x);
            int targetZ = Mathf.RoundToInt(hit.point.z);

            // basic validation
            if (!IsInsideGrid(targetX, targetZ)) return;
            if (IsObstacle(targetX, targetZ)) return;

            // get enemy grid position
            Vector2Int enemyTile = new Vector2Int(
                Mathf.RoundToInt(enemyTransform.position.x),
                Mathf.RoundToInt(enemyTransform.position.z)
            );

            // cannot move onto enemy tile
            if (targetX == enemyTile.x && targetZ == enemyTile.y) return;

            // forbid pathfinding from passing through enemy
            List<Vector2Int> forbiddenTiles = new List<Vector2Int>();
            forbiddenTiles.Add(enemyTile);

            // request path from central pathfinding manager
            List<Vector3> path = PathfindingManager.FindPath(
                new Vector2Int(currentX, currentZ),
                new Vector2Int(targetX, targetZ),
                obstacleData,
                forbiddenTiles
            );

            // start movement if valid path found
            if (path != null && path.Count > 0)
                StartCoroutine(MoveAlongPath(path));
        }
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

    // finds closest free tile around a blocked spawn
    Vector2Int FindNearestSafeTile(int x, int z)
    {
        for (int r = 1; r < 10; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dz = -r; dz <= r; dz++)
                {
                    int nx = x + dx;
                    int nz = z + dz;

                    if (!IsInsideGrid(nx, nz)) continue;
                    if (!IsObstacle(nx, nz)) return new Vector2Int(nx, nz);
                }
            }
        }
        return new Vector2Int(0, 0); // fallback
    }

    // moves player step by step along the computed path
    IEnumerator MoveAlongPath(List<Vector3> path)
    {
        isMoving = true;
        if (animator) animator.SetBool("IsWalking", true);

        // lock global movement
        AIManager.SetMovementState(true);

        foreach (var step in path)
        {
            Vector3 target = new Vector3(step.x, 0.9f, step.z);
            Vector3 dir = (target - transform.position).normalized;

            // rotate towards movement direction
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            // smooth movement to next tile
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, target, Time.deltaTime * 4.5f);
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

        // notify AI system that player finished moving
        FindFirstObjectByType<AIManager>()?
            .NotifyPlayerMoved(new Vector2Int(currentX, currentZ));
    }
}
