using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using Nav2D;

public class Nav2DGrid : MonoBehaviour
{
    Node[,] grid = null;
    List<Vector2> path = null;

    public Vector2 nodesize = new Vector2(0.5f, 0.5f);
    public Vector2Int dimension;
    public LayerMask notWalkableMask;

    [Header("SETTINGS")]
    public bool debug = true;

    private void CreateGrid()
    {
        dimension = Vector2Int.FloorToInt(transform.localScale / nodesize);
        dimension.x -= (dimension.x % 2 == 0) ? 0 : 1;
        dimension.y -= (dimension.y % 2 == 0) ? 0 : 1;
        grid = new Node[dimension.x, dimension.y];

        for (int x = 0; x < dimension.x; x++)
        {
            for (int y = 0; y < dimension.y; y++)
            {
                Vector3 worldPos = GridToWorldPosition(new Vector3(x, y));
                Node node = new Node(new Vector2(x, y));

                Collider2D hit = Physics2D.OverlapBox(worldPos, nodesize, 0, notWalkableMask);

                if (hit != null)
                {
                    node.walkable = false;
                }
                else
                {
                    node.walkable = true;
                }

                grid[x, y] = node;
            }
        }
    }

    public Vector3 GridToWorldPosition(Vector3 gridPosition)
    {
        //Vector3 pos = 
        //    transform.position - transform.localScale / 2 + 
        //    new Vector3(gridPosition.x * nodesize.x, gridPosition.y * nodesize.y) + 
        //    (Vector3) nodesize / 2;
        Vector3 pos = gridPosition - (Vector3) (Vector2) dimension / 2;
        pos.x = pos.x * nodesize.x + nodesize.x / 2;
        pos.y = pos.y * nodesize.y + nodesize.y / 2;
        pos += transform.position;
        return pos;
    }

    public List<Vector2> GetPath(Vector2 start, Vector2 end)
    {
        Node startnode = grid[(int) start.x, (int) start.y]; 
        Node endnode = grid[(int)end.x, (int)end.y]; 
        startnode.parent = null;
        startnode.gcost = 0;
        startnode.hcost = Node.Cost(startnode, endnode);

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();
        open.Add(startnode);

        while (true)
        {
            Node cnode = open[0];
            foreach (Node node in open) if (node.IsCostLessThan(cnode)) cnode = node;
            open.Remove(cnode);
            closed.Add(cnode);

            if (cnode.gridPosition == endnode.gridPosition)
            {
                List<Vector2> path = new List<Vector2>();
                while (true)
                {
                    path.Insert(0, endnode.gridPosition);
                    if (endnode.parent == null) break;
                    else endnode = endnode.parent;
                }
                return path;
            }

            foreach (Node neighbor in GetNodeNeighbors(cnode))
            {
                // if inclosed continue
                if (closed.Contains(neighbor))
                {
                    continue;
                }

                // if in open, compare else add
                int offeredg = cnode.gcost + Node.Cost(cnode, neighbor);
                if (open.Contains(neighbor))
                {
                    if (offeredg < neighbor.gcost)
                    {
                        neighbor.gcost = offeredg;
                        neighbor.parent = cnode;
                    }
                }
                else
                {
                    neighbor.gcost = offeredg;
                    neighbor.hcost = Node.Cost(neighbor, endnode);
                    neighbor.parent = cnode;
                    open.Add(neighbor);
                }
            }

            if (open.Count == 0)
            {
                List<Vector2> path = new List<Vector2>();
                while (true)
                {
                    path.Insert(0, cnode.gridPosition);
                    if (cnode.parent == null) break;
                    else cnode = cnode.parent;
                }
                return path;
            }
        }
    }

    public List<Node> GetNodeNeighbors(Node n)
    {
        List<Node> neighbors = new List<Node>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0) continue;

                int x = i + (int)n.gridPosition.x;
                int y = j + (int)n.gridPosition.y;

                if (x < 0 || x >= dimension.x || y < 0 || y >= dimension.y) continue;
                if (!grid[x, y].walkable) continue;

                neighbors.Add(grid[x, y]);
            }
        }
        return neighbors;
    }

    private void Start()
    {
        CreateGrid();
    }

    private void Update()
    {
        CreateGrid();
        if (true)
        {
            Vector2 end = new Vector2(dimension.x - 1, dimension.y - 1);
            path = GetPath(new Vector2(0, 0), end);
        }

    }

    private void OnDrawGizmos()
    {
        dimension = Vector2Int.FloorToInt(transform.localScale / nodesize);
        dimension.x -= (dimension.x % 2 == 0) ? 0 : 1;
        dimension.y -= (dimension.y % 2 == 0) ? 0 : 1;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        if (UnityEditor.Selection.activeGameObject == this.gameObject)
        {
            for (int x = 0; x < dimension.x; x++)
            {
                for (int y = 0; y < dimension.y; y++)
                {
                    Vector3 worldPos = GridToWorldPosition(new Vector3(x, y));
                    if (grid != null)
                    {
                        if (grid[x, y].walkable)
                        {
                            Gizmos.color = new Color(0, 0, 1, 0.1f);
                        }
                        else
                        {
                            Gizmos.color = new Color(1, 0, 0, 0.1f);
                        }
                        Gizmos.DrawCube(worldPos, nodesize);
                    }
                    Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
                    Gizmos.DrawWireCube(worldPos, nodesize);
                }
            }
        }

        if (path != null && debug)
        {
            foreach (Vector2 p in path)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(GridToWorldPosition(p), nodesize);
            }
        }
    }
}
