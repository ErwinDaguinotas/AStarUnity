using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using AStar;

public class Navigation2D : MonoBehaviour
{
    public GameObject tileMapObject;
    public Tilemap tilemap;
    public BoundsInt bounds;
    public TileBase[] map;

    [Header("debug")]
    public Vector2 start;
    public Vector2 end;

    public void ProcessTilemap()
    {
        tilemap = tileMapObject.GetComponent<Tilemap>();
        tilemap.CompressBounds();
        bounds = tilemap.cellBounds;
        map = tilemap.GetTilesBlock(bounds);
    }

    public List<Vector2> GetPath(Vector2 from, Vector2 to)
    {
        Vector2Int localfrom = Vector2Int.FloorToInt((Vector2)tilemap.WorldToLocal(from)) - (Vector2Int) bounds.position;
        Vector2Int localto = Vector2Int.FloorToInt((Vector2)tilemap.WorldToLocal(to)) - (Vector2Int) bounds.position;
        Debug.Log("before: " + localfrom);
        Debug.Log("before: " + localto);
        //Vector2Int localfrom = Vector2Int.FloorToInt((Vector2) from);
        //Vector2Int localto = Vector2Int.FloorToInt((Vector2) to);
        List<Vector2> path = DoAstar.GetPath(map, bounds.size.x, bounds.size.y, localfrom, localto);
        for (int i=0; i<path.Count; i++)
        {
            path[i] = path[i] + (Vector2) tilemap.GetLayoutCellCenter() + (Vector2Int) bounds.position;
        }
        return path;
    }

    void Start()
    {
        ProcessTilemap();
    }

    private void Awake()
    {
    }

    void Update()
    {
        if (tilemap.GetTilesBlock(bounds) != map) { ProcessTilemap(); }
        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    ProcessTilemap();
        //    Stack<Vector2> path = GetPath(start, end);
        //    //List<Vector2Int> path = DoAstar.GetPath(map, bounds.size.x, bounds.size.y, new Vector2Int(0, 0), new Vector2Int(bounds.size.x - 1, bounds.size.y - 1));
        //    if (path != null) 
        //    {
        //        Debug.Log("/////");
        //        foreach (Vector2 p in path)
        //        {
        //            Debug.Log(p);
        //        }
        //        Debug.Log("/////");
        //    }
        //    else Debug.Log("null uwu");
        //}
    }
}
