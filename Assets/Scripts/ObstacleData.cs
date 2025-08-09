using UnityEngine;

[System.Serializable]
public class BoolRow
{
    public bool[] row = new bool[10]; // 10 columns
}

[CreateAssetMenu(fileName = "ObstacleData", menuName = "ScriptableObjects/ObstacleData", order = 1)] //creating the assetfile
public class ObstacleData : ScriptableObject
{
    public BoolRow[] obstacles = new BoolRow[10]; // 10 rows



}
