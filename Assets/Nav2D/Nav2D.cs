using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nav2D
{
    public class Node
    {
        public Vector2 gridPosition;
        public Node parent;
        public bool walkable;
        public int weight;
        public int gcost;
        public int hcost;

        public Node(
            Vector2 _gridPosition,
            bool _walkable = false,
            Node _parent = null,
            int _weight = 0,
            int _gcost = 0,
            int _hcost = 0)
        {
            gridPosition = _gridPosition;
            parent = _parent;
            walkable = _walkable;
            weight = _weight;
            gcost = _gcost;
            hcost = _hcost;
        }

        public int fcost
        {
            get { return gcost + hcost; }
        }

        public static int Cost(Node a, Node b)
        {
            int x = Mathf.Abs((int)b.gridPosition.x - (int)a.gridPosition.x);
            int y = Mathf.Abs((int)b.gridPosition.y - (int)a.gridPosition.y);
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

        public Node GetCopy()
        {
            // copy resets to parent to null
            Node copy = new Node(gridPosition, walkable, null, weight, gcost, hcost);
            return copy;
        }
    }
}