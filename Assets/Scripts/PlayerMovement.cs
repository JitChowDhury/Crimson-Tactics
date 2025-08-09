using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private ObstacleData obstacleData; // Reference to obstacle grid
    [SerializeField] private Animator animator;

    //current grid pos of player
    private int currentX = 0;
    private int currentZ = 0;
    private bool isMoving = false;

    //stores the order of tiles the player will walk alomng
    private List<Vector3> path = new List<Vector3>();

    void Update()
    {
        //if not moving and mouse click
        if (!isMoving && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //checks if the ray hits something
            if (Physics.Raycast(ray, out hit))
            {
                //convert the hit point in world space to integer grid coordinates
                int targetX = Mathf.RoundToInt(hit.point.x);
                int targetZ = Mathf.RoundToInt(hit.point.z);
                //check if its inside grid and not a obstacle
                if (IsInsideGrid(targetX, targetZ) && !IsObstacle(targetX, targetZ))
                {
                    //using bfs we are gonna find path from current to target pos
                    path = FindPath(currentX, currentZ, targetX, targetZ);
                    //if vaild path then move
                    if (path != null && path.Count > 0)
                    {
                        StartCoroutine(MoveAlongPath(path));
                    }
                }
            }
        }
    }
    //checks if the coord is inside the grid
    bool IsInsideGrid(int x, int z)
    {
        return x >= 0 && x < 10 && z >= 0 && z < 10;
    }
    //check if obstacle
    bool IsObstacle(int x, int z)
    {
        if (obstacleData == null) return false;
        return obstacleData.obstacles[x].row[z];
    }
    //finds the shortest path
    List<Vector3> FindPath(int startX, int startZ, int targetX, int targetZ)
    {
        //queue for bfsto process tiles 
        Queue<Vector3> queue = new Queue<Vector3>();
        //dictionary to remember where each tile came from
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        //start and target pos
        Vector3 start = new Vector3(startX, 0, startZ);
        Vector3 target = new Vector3(targetX, 0, targetZ);
        //start bfs from the starting tile
        queue.Enqueue(start);
        cameFrom[start] = start;//mark it as visited

        //possible movement direction
        Vector3[] directions = {
            new Vector3(1,0,0), new Vector3(-1,0,0),
            new Vector3(0,0,1), new Vector3(0,0,-1)
        };

        while (queue.Count > 0)
        {
            //get the current tile
            Vector3 current = queue.Dequeue();
            //if reached then break
            if (current == target) break;
            //check all neighbor tile
            foreach (var dir in directions)
            {
                Vector3 neighbor = current + dir;
                int nx = (int)neighbor.x;
                int nz = (int)neighbor.z;
                // valid neighbor if inside grid not blocked and not visited yet

                if (IsInsideGrid(nx, nz) && !IsObstacle(nx, nz) && !cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor); // add to bfs
                    cameFrom[neighbor] = current;//remember where it came from
                }
            }
        }
        //if target wasnt visited there is no path
        if (!cameFrom.ContainsKey(target))
        {
            // No path found
            return null;
        }

        // reconstruct path by backtracking
        List<Vector3> finalPath = new List<Vector3>();
        Vector3 currentPathTile = target;

        while (currentPathTile != start)
        {
            //add each step to the front of the list (so it goes start → target)
            finalPath.Insert(0, currentPathTile);
            currentPathTile = cameFrom[currentPathTile];
        }

        return finalPath; //list of pos in order
    }

    //moves the player step by step along the calculated path
    IEnumerator MoveAlongPath(List<Vector3> path)
    {
        isMoving = true;//disable new movement
        animator.SetBool("IsWalking", true);

        foreach (var step in path)
        {
            //determine direction to the next step
            Vector3 direction = (new Vector3(step.x, 0.9f, step.z) - transform.position).normalized;
            //rotate towards the direction
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = lookRotation;
            }

            Vector3 targetPos = new Vector3(step.x, 0.9f, step.z);//target pos
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)//move until very close
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 5f);
                yield return null;//wait for next frame
            }
            //update current pos
            currentX = (int)step.x;
            currentZ = (int)step.z;
        }

        animator.SetBool("IsWalking", false);
        isMoving = false;//allow to move again
    }
}
