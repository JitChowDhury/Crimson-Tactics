using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [SerializeField] private ObstacleData obstacleData;
    [SerializeField] private GameObject obstacle;

    void Start()
    {
        PlaceObstacles();
    }

    void PlaceObstacles()
    {
        if (obstacleData == null || obstacle == null) return; //checks if the fields are empty . if then return and exit the func.

        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                if (obstacleData.obstacles[x].row[z])//checks if the box is true
                {
                    GameObject newObstacle = Instantiate(obstacle, new Vector3(x, 1f, z), Quaternion.identity);//if then instantiate a obstacle
                    newObstacle.GetComponent<Renderer>().material.color = Color.green;

                }
            }
        }
    }
}
