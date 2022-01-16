using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nav2D
{
    public class Node
    {
        public Vector2 worldPosition;
        public Vector2 gridPosition;
        public Node parent;
        public bool walkable;
        public int weight;
        public int gcost;
        public int hcost;

        public Node(
            Vector2 _worldPosition,
            Vector2 _gridPosition,
            bool _walkable = false,
            Node _parent = null,
            int _weight = 0,
            int _gcost = 0,
            int _hcost = 0)
        {
            worldPosition = _worldPosition;
            gridPosition = _gridPosition;
            parent = _parent;
            walkable = _walkable;
            weight = _weight;
            gcost = _gcost;
            hcost = _hcost;
        }

        public Node (Node a)
        {
            worldPosition = a.worldPosition;
            gridPosition = a.gridPosition;
            parent = a.parent;
            walkable = a.walkable;
            weight = a.weight;
            gcost = a.gcost;
            hcost = a.hcost;
        }

        public int fcost
        {
            get { return gcost + hcost; }
        }

        public static int Cost(Vector2 a, Vector2 b)
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
}