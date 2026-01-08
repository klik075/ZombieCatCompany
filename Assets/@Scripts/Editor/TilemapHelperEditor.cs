using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(TilemapHelper))]
public class TilemapHelperEditor : Editor
{
    void OnSceneGUI()
    {
        TilemapHelper helper = (TilemapHelper)target;
        if (helper.tilemap == null)
            return;

        Tilemap tilemap = helper.tilemap;
        BoundsInt bounds = tilemap.cellBounds;
        Vector3 cellSize = tilemap.cellSize;

        Handles.color = Color.yellow;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 12;
        style.alignment = TextAnchor.MiddleCenter;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (tilemap.GetTile(cell) == null)
                    continue;

                Vector3 center = tilemap.GetCellCenterWorld(cell);
                Handles.Label(center, $"({x},{y})", style);
            }
        }
    }
}
