using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages threaded data requests and their corresponding callbacks.
/// </summary>
public class ThreadedDataRequester : MonoBehaviour
{
    private static ThreadedDataRequester instance;
    private Queue<ThreadInfo> dataQueue = new();

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }

    /// <summary>
    /// Requests data on a separate thread and specifies a callback for when the data is ready.
    /// </summary>
    /// <param name="generateData">Function to generate the requested data.</param>
    /// <param name="callback">Callback to be invoked with the generated data.</param>
    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.DataThread(generateData, callback);
        };

        new Thread(threadStart).Start();
    }

    /// <summary>
    /// Worker thread method to generate data and enqueue it along with the callback.
    /// </summary>
    /// <param name="generateData">Function to generate the requested data.</param>
    /// <param name="callback">Callback to be invoked with the generated data.</param>
    private void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();

        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    /// <summary>
    /// Update is called once per frame, processes enqueued data and invokes callbacks.
    /// </summary>
    private void Update()
    {
        while (dataQueue.Count > 0)
        {
            ThreadInfo threadInfo = dataQueue.Dequeue();
            threadInfo.callback(threadInfo.parameter);
        }
    }

    /// <summary>
    /// Structure to hold information about a threaded data request.
    /// </summary>
    private struct ThreadInfo
    {
        /// <summary>
        /// Callback to be invoked with the generated data.
        /// </summary>
        public readonly Action<object> callback;

        /// <summary>
        /// The generated data or parameter for the callback.
        /// </summary>
        public readonly object parameter;

        /// <summary>
        /// Initializes a new instance of the ThreadInfo structure.
        /// </summary>
        /// <param name="callback">Callback to be invoked with the generated data.</param>
        /// <param name="parameter">The generated data or parameter for the callback.</param>
        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}