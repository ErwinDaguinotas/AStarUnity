using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nav2DAgent : MonoBehaviour
{
    public GameObject gridObject;
    public Nav2DGrid grid;
    public List<Vector3> path = null;
    public Transform target;
    public Rigidbody2D rb;
    public float speed;

    void Start()
    {
        grid = gridObject.GetComponent<Nav2DGrid>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (true)//Input.GetKeyDown(KeyCode.Space))
        {
            path = grid.RequestPath(transform.position, target.position);
            path.RemoveAt(0); 
        }
    }

    private void FixedUpdate()
    {
        if (path != null && path.Count > 0)
        {
            Vector3 direction = (path[0] - transform.position).normalized;
            rb.velocity = direction * speed;
            //transform.position = Vector3.MoveTowards(transform.position, path[0], speed * Time.deltaTime);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (path != null && path.Count > 0)
        {
            for (int i=0; i<path.Count-1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
            Gizmos.DrawLine(transform.position, path[0]);
        }
    }
}
