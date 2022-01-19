using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;
using System.Threading;
using System;

using Nav2D;

public class Nav2DGrid : MonoBehaviour
{
    Node[,] grid = null;

    [Header("GRID SETTINGS")]
    public Vector3 position;
    public Vector3 size;
    public Vector2 nodesize = new Vector2(0.5f, 0.5f);
    public Vector2Int dimension;
    public bool shouldRecreateGrid = true;
    public LayerMask notWalkableMask;

    [Header("PATH REQUEST SYSTEM")]
    public Queue<PathRequest> requests;
    public List<Thread> runningRequests;
    public int maxRunningRequests = 5;

    // ===UNITY FUNCTIONS===
    private void Awake()
    {
        requests = new Queue<PathRequest>();
        runningRequests = new List<Thread>();
    }

    private void Start()
    {
        CreateGrid();
        new Thread(DoRequests).Start();
    }

    void Update()
    {
        CreateGrid();
    }


    // ===PATH REQEUSTS MANAGER===
    public void RequestPath(PathRequest req)
    {
        requests.Enqueue(req);
    }

    public void DoRequests()
    {
        while(true)
        {
            // removing finished threads
            List<Thread> temp = new List<Thread>();
            foreach (Thread running in runningRequests)
                if (running.IsAlive)
                    temp.Add(running);
            lock (runningRequests) { runningRequests = temp; }

            if (requests.Count > 0 && runningRequests.Count < maxRunningRequests)
            {
                PathRequest req;
                lock (requests) { req = requests.Dequeue(); }

                Thread t = new Thread(() => { GetPath((Node[,])grid.Clone(), req); });
                t.Start();

                lock(runningRequests) { runningRequests.Add(t); }
            }
        }
    }
    

    // ===FUNCTIONS===
    public void UpdateGridSettings()
    {
        if (position != transform.position)
        {
            position = transform.position;
            shouldRecreateGrid = true;
        }
        if (size != transform.localScale)
        {
            size = transform.localScale;
            shouldRecreateGrid = true;
        }
        dimension = Vector2Int.FloorToInt(size / nodesize);
        dimension.x -= (dimension.x % 2 == 0) ? 0 : 1;
        dimension.y -= (dimension.y % 2 == 0) ? 0 : 1;
    }

    private void CreateGrid()
    {
        UpdateGridSettings();

        if (!shouldRecreateGrid) return;
        shouldRecreateGrid = false;

        Node[,] newgrid = new Node[dimension.x, dimension.y];
        for (int x = 0; x < dimension.x; x++)
        {
            for (int y = 0; y < dimension.y; y++)
            {
                Vector3 worldPos = GridToWorldPosition(new Vector2Int(x, y));
                Node node = new Node(new Vector2Int(x, y));

                Collider2D hit = Physics2D.OverlapBox(worldPos, nodesize, 0, notWalkableMask);

                if (hit != null)
                {
                    node.walkable = false;
                }
                else
                {
                    node.walkable = true;
                }

                newgrid[x, y] = node;
            }
        }
        if (grid != null)
            lock (grid)
            {
                grid = newgrid;
            }
        else grid = newgrid;
    }

    public void GetPath(Node[,] grid, PathRequest req)
    {
        // cloning grid
        for (int i=0; i<grid.GetLength(0); i++)
        {
            for (int j=0; j<grid.GetLength(1); j++)
            {
                grid[i, j] = grid[i, j].GetClone();
            }
        }

        Vector2Int startVec = WorldToGridPosition(req.startPosition);
        Vector2Int endVec = WorldToGridPosition(req.endPosition);
        Node start = grid[startVec.x, startVec.y];
        Node end = grid[endVec.x, endVec.y];
        start.parent = start;
        start.gcost = 0;
        start.hcost = Node.Cost(start.position, start.position);

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();
        open.Add(start);

        List<Vector3> path = new List<Vector3>();

        while (true)
        {
            Node cnode = open[0];
            foreach (Node n in open) if (n.IsCostLessThan(cnode)) cnode = n;
            open.Remove(cnode);
            closed.Add(cnode);

            if (cnode.position == end.position)
            {
                // retrace the path
                while (true)
                {
                    path.Insert(0,GridToWorldPosition(cnode.position));
                    if (cnode.position == cnode.parent.position) break;
                    else cnode = cnode.parent;
                }
            }

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int x = i + cnode.position.x;
                    int y = j + cnode.position.y;

                    if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1)) continue;
                    if (!grid[x, y].walkable) continue;

                    Node neighbor = grid[x, y];

                    if (closed.Contains(neighbor)) continue;

                    int offeredg = cnode.gcost + Node.Cost(cnode.position, neighbor.position);
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
                        neighbor.hcost = Node.Cost(neighbor.position, end.position);
                        neighbor.parent = cnode;
                        open.Add(neighbor);
                    }
                }
            }

            if (open.Count == 0)
            {
                path.Insert(0,GridToWorldPosition(start.position));
                break;
            }
        }

        // simplifying path
        List<Vector3> simplifiedPath = new List<Vector3>();
        Vector3 oldDirection = Vector3.zero;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 newDirection = path[i + 1] - path[i];
            if (newDirection != oldDirection)
            {
                simplifiedPath.Add(path[i]);
                oldDirection = newDirection;
            }
        }
        simplifiedPath.Add(path[path.Count - 1]);

        lock (req)
        {
            req.path = simplifiedPath;
            req.isDone = true;
        }
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        //Vector3 pos = 
        //    transform.position - transform.localScale / 2 + 
        //    new Vector3(gridPosition.x * nodesize.x, gridPosition.y * nodesize.y) + 
        //    (Vector3) nodesize / 2;
        Vector3 pos = (Vector3)(Vector2)(gridPosition - dimension / 2);
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

    // ===GIZMOS===
    private void OnDrawGizmos()
    {
        UpdateGridSettings();
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
