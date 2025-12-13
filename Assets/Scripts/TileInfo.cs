using UnityEngine;

public class TileInfo : MonoBehaviour
{
    private int x, z; // grid coords

    public void SetPosition(int xPos, int zPos) // called by gridgenerator
    {
        x = xPos;
        z = zPos;
    }

    public int GetX() { return x; }
    public int GetZ() { return z; }
}