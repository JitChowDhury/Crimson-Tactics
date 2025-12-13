using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [SerializeField] private ObstacleData obstacleData;//scriptable object
    [SerializeField] private GameObject obstaclePrefab;

    void Start()
    {
        PlaceObstacles();
    }
    //reads obstacle data and spawn onstacles
    void PlaceObstacles()
    {
        if (obstacleData == null || obstaclePrefab == null) return;

        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                if (obstacleData.obstacles[x].row[z])
                {
                    Instantiate(obstaclePrefab, new Vector3(x, 1f, z), Quaternion.identity);
                }
            }
        }
    }
}
