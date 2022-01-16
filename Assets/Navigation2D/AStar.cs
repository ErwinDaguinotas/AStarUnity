using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AStar
{
    public class Node
    {
        private Vector2Int coordinate;
        private Node parent;
        private int gcost;
        private int hcost;
        private int fcost;

        public Node(Vector2Int coordinate, Node parent = null)
        {
            this.coordinate = coordinate;
            this.parent = (parent == null)? this : parent;
        }

        public static int Cost(Node a, Node b)
        {
            int x = Mathf.Abs(b.coordinate.x - a.coordinate.x);
            int y = Mathf.Abs(b.coordinate.y - a.coordinate.y);
            int c = Mathf.Max(x, y) + Mathf.Min(x, y) * (14 - 10);
            return c;
        }

        public int GCost
        {
            get { return gcost; }
            set { gcost = value; fcost = gcost + hcost; }
        }

        public int HCost
        {
            get { return hcost; }
            set { hcost = value; fcost = gcost + hcost; }
        }

        public int FCost
        {
            get { return fcost; }
        }

        public Node Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public Vector2Int Coordinate
        {
            get { return coordinate; }
            set { coordinate = value; }
        }

        public int CalcF()
        {
            fcost = gcost + hcost;
            return fcost;
        }

        public bool IsCostLessThan(Node a)
        {
            if (fcost == a.fcost)
                if (hcost == a.hcost) return gcost < a.gcost;
                else return hcost < a.hcost;
            else return fcost < a.fcost;
        }

        public bool isNodeEqualTo(Node a)
        {
            return a.coordinate == coordinate;
        }
    }

    public static class DoAstar
    {
        public static List<Vector2> GetPath(TileBase[] map, int w, int h, Vector2Int start, Vector2Int end)
        {
            if (start.x < 0 || start.x >= w || start.y < 0 || start.y >= h) return null;
            if (end.x < 0 || end.x >= w || end.y < 0 || end.y >= h) return null;

            Node startnode = new Node(start);
            Node endnode = new Node(end);
            int cost = Node.Cost(startnode, endnode);
            startnode.GCost = 0;
            startnode.HCost = cost;
            startnode.Parent = startnode;
            endnode.GCost = cost;
            endnode.HCost = 0;

            List<Node> open = new List<Node>();
            List<Node> closed = new List<Node>();
            open.Add(startnode);

            bool found = true;

            while (true)
            {
                Node cnode = open[0];
                foreach (Node node in open)
                    if (node.IsCostLessThan(cnode)) cnode = node;
                open.Remove(cnode);
                closed.Add(cnode);

                if (cnode.Coordinate == endnode.Coordinate)
                {
                    endnode = cnode;
                    break;
                }

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        // self
                        if (i == 0 && j == 0) continue;

                        int x = i + cnode.Coordinate.x;
                        int y = j + cnode.Coordinate.y;

                        // out of bounds
                        if (x < 0 || x >= w || y < 0 || y >= h) continue;
                        int index = y * w + x;
                        if (map[index] == null) continue;

                        Vector2Int coordinate = new Vector2Int(x, y);

                        // if inclosed continue
                        bool inClosed = false;
                        foreach (Node node in closed)
                        {
                            if (node.Coordinate == coordinate)
                            {
                                inClosed = true;
                                break;
                            }
                        }
                        if (inClosed) continue;

                        // if in open, compare else add
                        int offeredg = cnode.GCost + ((i == 0 || j == 0) ? 10 : 14);
                        bool inOpen = false;
                        foreach (Node node in open)
                        {
                            if (node.Coordinate == coordinate)
                            {
                                if (offeredg < node.GCost)
                                {
                                    node.GCost = offeredg;
                                    node.Parent = cnode;
                                }

                                inOpen = true;
                                break;
                            }
                        }
                        if (inOpen) continue;

                        Node n = new Node(coordinate);
                        n.GCost = offeredg;
                        n.HCost = Node.Cost(n, endnode);
                        n.Parent = cnode;
                        open.Add(n);
                    }
                }
                if (open.Count == 0)
                {
                    found = false;
                    break;
                }
            }

            if (found)
            {
                List<Vector2> solution = new List<Vector2>();
                while (true)
                {
                    solution.Insert(0,endnode.Coordinate);

                    if (endnode.Coordinate == endnode.Parent.Coordinate)
                    {
                        break;
                    }
                    endnode = endnode.Parent;
                }
                return solution;
            }

            return null;
        }
    }
}
