using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;
using System.Threading;


using Nav2D;

public class Nav2DGrid : MonoBehaviour
{
    Node[,] grid = null;

    [Header("FOR THREADING")]
    public Vector3 position;
    public Vector3 size;

    [Header("SETTINGS")]
    public Vector2 nodesize = new Vector2(0.5f, 0.5f);
    public Vector2Int dimension;
    public LayerMask notWalkableMask;

    [Header("PATH REQUEST SYSTEM")]
    public Queue<PathRequest> requests;
    public Queue<Thread> runningRequests;
    public Nav2DGrid instance;

    private void Start()
    {
        requests = new Queue<PathRequest>();
        runningRequests = new Queue<Thread>();
        instance = this;

        CreateGrid();
    }

    
    // ===PATH REQEUSTS MANAGER===
    public void RequestPath(PathRequest req)
    {
        Debug.Log("RequestPath() -- request recieved");
        requests.Enqueue(req);
        Debug.Log("RequestPath() -- request queued");
    }

    public void DoRequests()
    {
        while (requests.Count > 0)
        {
            Debug.Log("DoRequests() -- request threaded");
            PathRequest req = requests.Dequeue();
            Thread t = new Thread(() =>
            {
                GetPath((Node[,])grid.Clone(), req);
            });
            t.Start();
            Debug.Log("DoRequests() -- request thread started");
            // add to running requests
        }
    }
    void Update()
    {
        CreateGrid();
        DoRequests();
    }


    // ===UWU===

    private void CreateGrid()
    {
        position = transform.position;
        size = transform.localScale;
        dimension = Vector2Int.FloorToInt(transform.localScale / nodesize);
        dimension.x -= (dimension.x % 2 == 0) ? 0 : 1;
        dimension.y -= (dimension.y % 2 == 0) ? 0 : 1;
        grid = new Node[dimension.x, dimension.y];

        for (int x = 0; x < dimension.x; x++)
        {
            for (int y = 0; y < dimension.y; y++)
            {
                Vector3 worldPos = GridToWorldPosition(new Vector2Int(x, y));
                Node node = new Node(new Vector2Int(x, y), new Vector2Int(x, y));

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

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        //Vector3 pos = 
        //    transform.position - transform.localScale / 2 + 
        //    new Vector3(gridPosition.x * nodesize.x, gridPosition.y * nodesize.y) + 
        //    (Vector3) nodesize / 2;
        Vector3 pos = (Vector3) (Vector2) (gridPosition - dimension / 2);
        pos.x = pos.x * nodesize.x + nodesize.x / 2;
        pos.y = pos.y * nodesize.y + nodesize.y / 2;
        pos += position;
        return pos;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector2 pos = worldPosition - position;
        pos.x = (pos.x) / nodesize.x;
        pos.y = (pos.y) / nodesize.y;
        pos += (Vector2)dimension / 2;
        return Vector2Int.FloorToInt(pos);
    }

    public void GetPath(Node[,] grid, PathRequest req)
    {
        Debug.Log("GetPath() -- request thread started");
        Vector2Int start = WorldToGridPosition(req.startPosition);
        Vector2Int end = WorldToGridPosition(req.endPosition);
        grid[start.x, start.y].parent = new Vector2Int(start.x, start.y);
        grid[start.x, start.y].gcost = 0;
        grid[start.x, start.y].hcost = Node.Cost(start, end);

        List<Vector2Int> open = new List<Vector2Int>();
        List<Vector2Int> closed = new List<Vector2Int>();
        open.Add(start);

        List<Vector3> path = new List<Vector3>();

        while (true)
        {
            Vector2Int cnode = open[0];
            foreach (Vector2Int n in open) if (grid[n.x,n.y].IsCostLessThan(grid[cnode.x,cnode.y])) cnode = n;
            open.Remove(cnode);
            closed.Add(cnode);

            if (cnode == end)
            {
                lock (req)
                {
                    req.msg = "path is found";
                }
                // retrace the path
                while (true)
                {
                    path.Insert(0, GridToWorldPosition(grid[cnode.x,cnode.y].position));
                    if (cnode == grid[cnode.x, cnode.y].parent) break;
                    else cnode = grid[cnode.x, cnode.y].parent;
                }
            }

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int x = i + cnode.x;
                    int y = j + cnode.y;

                    if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1)) continue;
                    if (!grid[x, y].walkable) continue;

                    Vector2Int neighbor = new Vector2Int(x, y);

                    if (closed.Contains(neighbor)) continue;

                    int offeredg = grid[cnode.x,cnode.y].gcost + Node.Cost(cnode, neighbor);
                    if (open.Contains(neighbor))
                    {
                        if (offeredg < grid[neighbor.x,neighbor.y].gcost)
                        {
                            grid[neighbor.x, neighbor.y].gcost = offeredg;
                            grid[neighbor.x, neighbor.y].parent = cnode;
                        }
                    }
                    else
                    {
                        grid[neighbor.x, neighbor.y].gcost = offeredg;
                        grid[neighbor.x, neighbor.y].hcost = Node.Cost(neighbor, end);
                        grid[neighbor.x, neighbor.y].parent = cnode;
                        open.Add(neighbor);
                    }
                }
            }

            if (open.Count == 0)
            {
                path.Insert(0, GridToWorldPosition(start));
                lock(req)
                {
                    req.msg = "no path is found";
                }
                break;
            }
        }

        lock(req)
        {
            req.path = path;
            req.isDone = true;
        }

        Debug.Log("GetPath() -- request thread ended");
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
                    Vector3 worldPos = GridToWorldPosition(new Vector2Int(x, y));
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
    }
}
