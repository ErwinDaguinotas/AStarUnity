using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nav2D;

public class Nav2DAgent : MonoBehaviour
{
    public GameObject gridObject;
    Nav2DGrid grid;

    public Transform target;
    public float positionThreshold;
    public float speed;
    Vector3 targetLastPosition;
    Rigidbody2D rb;

    public List<Vector3> path = null;
    PathRequest req;

    public System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    // ===UNITY FUNCTIONS===
    void Start()
    {
        grid = gridObject.GetComponent<Nav2DGrid>();
        rb = GetComponent<Rigidbody2D>();

        req = null;
        StartCoroutine(FollowPath());
        StartCoroutine(RequestPath());
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
    }

    // ===PATH===
    IEnumerator FollowPath()
    {
        while (true)
        {
            lock (path)
            {
                if (path != null && path.Count > 0)
                {
                    Vector3 direction = (path[0] - transform.position).normalized;
                    rb.velocity = direction * speed;
                    if (grid.WorldToGridPosition(transform.position) == grid.WorldToGridPosition(path[0]))
                    {
                            path.RemoveAt(0);
                    }
                }
                else
                {
                    rb.velocity = Vector3.zero;
                }
            }
            yield return null;
        }
    }

    IEnumerator RequestPath()
    {
        while (true)
        {
            // if path is complete
            if (req != null && req.isDone == true)
            {
                timer.Stop();
                Debug.Log("Path found! Time taken: " + timer.Elapsed);
                timer.Reset();

                // merge new path with existing path

                lock(path)
                {
                    path = req.path;
                }
                lock(req)
                {
                    req = null;
                }
            }
            // if should request for a new path
            else if (req == null && (path == null || path.Count == 0|| (target.transform.position - targetLastPosition).magnitude > positionThreshold))
            {
                timer.Start();
                req = new PathRequest(this, transform.position, target.transform.position);
                grid.RequestPath(req);
                targetLastPosition = target.transform.position;
            }
            yield return null;
        }
    }

    // ===FUNCTIONS===
    void DrawPath()
    {
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
            Gizmos.DrawLine(transform.position, path[0]);
        }
    }

    // ===GIZMOS===
    private void OnDrawGizmos()
    {
        DrawPath();
    }
}
