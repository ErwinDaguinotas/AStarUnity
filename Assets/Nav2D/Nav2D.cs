using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nav2D
{
    public class Node
    {
        public Vector2Int position;
        public Vector2Int parent;
        public bool walkable;
        public int weight;
        public int gcost;
        public int hcost;

        public Node(
            Vector2Int _position,
            Vector2Int _parent,
            bool _walkable = false,
            int _weight = 0,
            int _gcost = 0,
            int _hcost = 0)
        {
            position = _position;
            parent = _parent;
            walkable = _walkable;
            weight = _weight;
            gcost = _gcost;
            hcost = _hcost;
        }

        public int fcost { get { return gcost + hcost + weight; } }

        public static int Cost(Vector2Int a, Vector2Int b)
        {
            int x = Mathf.Abs((int)b.x - (int)a.x);
            int y = Mathf.Abs((int)b.y - (int)a.y);
            int c = Mathf.Max(x, y) * 10 + Mathf.Min(x, y) * (14 - 10);
            return c;
        }

        public bool IsCostLessThan(Node a)
        {
            if (fcost == a.fcost)
                if (hcost == a.hcost) return gcost < a.gcost;
                else return hcost < a.hcost;
            else return fcost < a.fcost;
        }
    }

    public class PathRequest
    {
        public Nav2DAgent owner;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public bool isDone;
        public List<Vector3> path;
        public string msg;

        public PathRequest(
            Nav2DAgent _owner, 
            Vector3 _startPosition, 
            Vector3 _endPosition, 
            bool _isDone=false, 
            List<Vector3> _path=null)
        {
            owner = _owner;
            startPosition = _startPosition;
            endPosition = _endPosition;
            isDone = _isDone;
            path = _path;
            msg = "none";
        }
    }

    public struct Heap
    {
    }
}