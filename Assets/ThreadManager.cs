using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class ThreadManager : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("start -- started");
        StartThreadedFunction(() => { slowFunction(new Vector3(1, 1, 1), 10.0f); });
        Debug.Log("end -- end");
    }


    public void StartThreadedFunction( Action someFunction )
    {
        Thread t = new Thread(new ThreadStart( someFunction ));
        t.Start();
    }

    List<Action> actionsToRun = new List<Action>();

    public void QueueMainThreadFunction(Action someFunction)
    {
        actionsToRun.Add(someFunction);
    }

    void slowFunction(Vector3 v, float f)
    {
        Thread.Sleep(2000);

        Action func = () =>
        {
            this.transform.position = new Vector3(1, 1, 1);
            Debug.Log("slow function");
        };

        QueueMainThreadFunction(func);

    }

    private void Update()
    {
        while (actionsToRun.Count > 0)
        {
            Action someFunc = actionsToRun[0];
            actionsToRun.RemoveAt(0);
            someFunc();
        }
    }

}
