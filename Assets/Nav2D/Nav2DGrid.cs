using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using Nav2D;

public class Nav2DGrid : MonoBehaviour
{
    public Node[,] grid;

    public Vector3 position;
    public float sizex;
    public float sizey;

    public float nodesizex;
    public float nodesizey;
    public int dimensionx;
    public int dimensiony;

    List<Vector2> path = null;

    public LayerMask notWalkableMask;

    private void CreateGrid()
    {
        dimensionx = Mathf.FloorToInt(sizex/nodesizex);
        dimensiony = Mathf.FloorToInt(sizey/nodesizey);
        grid = new Node[dimensionx, dimensiony];

        for (int x = 0; x < dimensionx; x++)
        {
            for (int y = 0; y < dimensiony; y++)
            {
                Vector3 worldPos = GridToWorldPosition(new Vector3(x, y));
                Node gridnode = new Node(worldPos, new Vector3(x, y));

                Collider2D hit = Physics2D.OverlapBox(worldPos, new Vector2(nodesizex,nodesizey), 0, notWalkableMask);

                if (hit != null)
                {
                    gridnode.walkable = false;
                }
                else
                {
                    gridnode.walkable = true;
                }

                grid[x, y] = gridnode;
            }
        }
    }

    public Vector3 GridToWorldPosition(Vector3 gridPosition)
    {
        float px = position.x - sizex / 2 + (gridPosition.x) * nodesizex + nodesizex / 2;
        float py = position.y - sizey / 2 + (gridPosition.y) * nodesizey + nodesizey / 2;
        return new Vector3(px, py);
    }

    public List<Vector2> GetPath(Vector2 start, Vector2 end)
    {
        int cost = Node.Cost(start, end);
        Node startnode = grid[(int) start.x, (int) start.y]; startnode.parent = null;
        Node endnode = grid[(int)end.x, (int)end.y];

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();
        open.Add(startnode);

        while (true)
        {
            Node cnode = open[0];
            foreach (Node node in open)
                if (node.IsCostLessThan(cnode)) cnode = node;
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
                int offeredg = cnode.gcost + Node.Cost(cnode.gridPosition, neighbor.gridPosition);
                //int offeredg = cnode.gcost + ((cnode.gridPosition.x-neighbor.gridPosition.x==0 || cnode.gridPosition.y - neighbor.gridPosition.y == 0)?10:14);

                if (open.Contains(neighbor))
                {
                    if (neighbor.gcost > offeredg)
                    {
                        neighbor.gcost = offeredg;
                        neighbor.parent = cnode;
                    }
                }
                else
                {
                    neighbor.gcost = offeredg;
                    neighbor.hcost = Node.Cost(neighbor.gridPosition, endnode.gridPosition);
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
        return null;
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

                if (x < 0 || x >= dimensionx || y < 0 || y >= dimensiony) continue;
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
        if (true)//Input.GetKeyDown(KeyCode.Return))
        {
            Vector2 end = new Vector2(dimensionx - 1, dimensiony - 1);
            Debug.Log(end);
            path = GetPath(new Vector2(0, 0), end);
            if (path != null)
            {
                foreach (Vector2 p in path)
                {
                    Debug.Log(p);
                }
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(position, new Vector2(sizex, sizey));
        for (int x = 0; x < dimensionx; x++)
        {
            for (int y = 0; y < dimensiony; y++)
            {
                Vector3 worldPos = GridToWorldPosition(new Vector3(x, y));
                if (grid[x, y].walkable)
                {
                    Gizmos.color = new Color(0, 0, 1, 0.1f);
                }
                else
                {
                    Gizmos.color = new Color(1, 0, 0, 0.1f);
                }
                Gizmos.DrawCube(worldPos, new Vector2(nodesizex, nodesizey));
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(worldPos, new Vector2(nodesizex, nodesizey));
            }
        }

        if (path != null)
        {
            foreach (Vector2 p in path)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(GridToWorldPosition(p), new Vector2(nodesizex, nodesizey));
            }
        }
    }
}
