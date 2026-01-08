using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapHelper : MonoBehaviour
{
    public Tilemap tilemap;

    void Reset()
    {
        tilemap = GetComponent<Tilemap>();
    }
}
