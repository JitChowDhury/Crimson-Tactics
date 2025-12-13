using System.Collections.Generic;
using UnityEngine;

public static class PathfindingManager
{
    //Internal node class used by A* to store path data
    private class Node
    {
        public Vector2Int pos;   //grid position of this node
        public int gCost;        //cost from start to this node
        public int hCost;        //estimated cost to target
        public int fCost => gCost + hCost; //total cost
        public Node parent;      //previous node (for path reconstruction)

        public Node(Vector2Int p, int g, int h, Node parent)
        {
            pos = p;
            gCost = g;
            hCost = h;
            this.parent = parent;
        }
    }

    //Finds a path from start to target using A* pathfinding
    //forbiddenTiles is used to block dynamic units (player/enemy)
    public static List<Vector3> FindPath(
        Vector2Int start,
        Vector2Int target,
        ObstacleData obstacleData,
        List<Vector2Int> forbiddenTiles
    )
    {
        // prevent walking onto forbidden tile
        if (forbiddenTiles != null && forbiddenTiles.Contains(target))
            return null;

        // prevent walking onto obstacle
        if (obstacleData.obstacles[target.x].row[target.y])
            return null;

        // open list = nodes to be evaluated
        List<Node> open = new List<Node>();

        // closed set = already evaluated positions
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();

        // create starting node
        Node startNode = new Node(start, 0, Heuristic(start, target), null);
        open.Add(startNode);

        // 4directional movement
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        // main A* loop
        while (open.Count > 0)
        {
            // pick node with lowest fCost (and lower hCost as tiebreaker)
            Node current = open[0];
            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].fCost < current.fCost ||
                   (open[i].fCost == current.fCost && open[i].hCost < current.hCost))
                {
                    current = open[i];
                }
            }

            // reached target - build path
            if (current.pos == target)
                return ReconstructPath(current);

            // move node from open to closed
            open.Remove(current);
            closed.Add(current.pos);

            // check neighbors
            foreach (var d in dirs)
            {
                Vector2Int next = current.pos + d;

                // outside grid bounds
                if (next.x < 0 || next.x > 9 || next.y < 0 || next.y > 9)
                    continue;

                // blocked by obstacle
                if (obstacleData.obstacles[next.x].row[next.y])
                    continue;

                // blocked by dynamic unit
                if (forbiddenTiles != null && forbiddenTiles.Contains(next))
                    continue;

                // already evaluated
                if (closed.Contains(next))
                    continue;

                int newG = current.gCost + 1;

                // check if node already exists in open list
                Node existing = open.Find(n => n.pos == next);

                if (existing == null)
                {
                    // add new node
                    Node node = new Node(
                        next,
                        newG,
                        Heuristic(next, target),
                        current
                    );
                    open.Add(node);
                }
                else if (newG < existing.gCost)
                {
                    // better path found - update node
                    existing.gCost = newG;
                    existing.parent = current;
                }
            }
        }

        // no path found
        return null;
    }

    // Manhattan distance heuristic gridfriendly
    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // Builds final path by backtracking from target node
    private static List<Vector3> ReconstructPath(Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node current = endNode;

        // walk backwards using parent references
        while (current.parent != null)
        {
            path.Insert(0, new Vector3(current.pos.x, 0, current.pos.y));
            current = current.parent;
        }

        return path;
    }
}
