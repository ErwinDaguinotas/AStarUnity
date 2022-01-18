using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading;

public class thread : MonoBehaviour
{
    public int counter = 0;

    Thread mythread;

    // Start is called before the first frame update
    void Start()
    {
        mythread = new Thread(SlowJob);
    }

    void SlowJob()
    {
        Debug.Log("slow job start");
        Stopwatch timer = new Stopwatch();
        timer.Start();
        for (int i=0; i<500; i++)
        {
            Thread.Sleep(2);
            //transform.Translate(Vector2.up * Time.deltaTime);
        }
        timer.Stop();

        counter++;
        Debug.Log("slow job " + timer.Elapsed);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        { 
            Debug.Log("starting");
            mythread = new Thread(SlowJob);
            mythread.Start();
            Debug.Log("end");
        }

        if (mythread.IsAlive) Debug.Log("is running");

    }
}
