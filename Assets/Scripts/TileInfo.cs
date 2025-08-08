using UnityEngine;

public class TileInfo : MonoBehaviour
{
    private int x, z;

    public void SetPosition(int xPos, int zPos)
    {
        x = xPos;
        z = zPos;
    }

    public int GetX() { return x; }
    public int GetZ() { return z; }
}