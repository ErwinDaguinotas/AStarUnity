using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nav2D;

public class Nav2DAgent : MonoBehaviour
{
    public GameObject gridObject;
    public Nav2DGrid grid;

    public Transform target;
    public Vector3 targetLastPosition;
    public float positionThreshold;
    public Rigidbody2D rb;
    public float speed;


    public List<Vector3> path = null;
    public PathRequest req;

    void Start()
    {
        grid = gridObject.GetComponent<Nav2DGrid>();
        rb = GetComponent<Rigidbody2D>();

        req = null;
    }

    void FollowPath()
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

    void RequestPath()
    {
        // check if should request for a path
        if (req != null && req.isDone == true)
        {
            // copy the resulting path to this.path
            path = req.path;
            //Debug.Log("RequestPath() -- request done");
            //Debug.Log("RequestPath() -- MSG: " + req.msg);
            req = null;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        //else if ((path.Count <= 0  || (target.transform.position-targetLastPosition).magnitude > positionThreshold) && req == null)
        {
            // request for a path
            //Debug.Log("RequestPath() -- path requested");
            req = new PathRequest(this, transform.position, target.transform.position);
            grid.RequestPath(req);
        }
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
        RequestPath();
        FollowPath();
    }

    private void OnDrawGizmos()
    {
        DrawPath();
    }
}
