using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ProjectSearchManager : MonoBehaviour
{
    private static int Timeout = 60 * 1000; // 1 min
    SynchronizationContext context;

    private void Awake() => context = SynchronizationContext.Current;

    private async void Start()
    {
        everything = new ProjectFinder();
        await StartSearchEngineAsync();
    }

    private async Task StartSearchEngineAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        await Task.Run(() =>
         {
             if (!everything.IsStarted())
             {
                 everything.StartService();
             }

             while (!everything.IsReady() && stopwatch.ElapsedMilliseconds < Timeout)
             {
                 Thread.Sleep(1000);
             }
         });
        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds > Timeout)
        {
            Debug.LogError("Could not start Everything process");
        }
        else
        {
            Debug.Log($"Everything version: {everything.Version}");
        }
    }

    ProjectFinder everything;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryQueryProjectsAsync();
        }
    }
    private void TryQueryProjectsAsync()
    {
        Task.Run(async () =>
        {
            await TaskEx.WaitUntil(everything.IsReady);
            var result = await everything.SearchProjectAsync();
            if (result != null)
            {
                Debug.Log($"Result {result.Count}");
                foreach (var item in result)
                {
                    Debug.Log(item);
                }
            }
        });
    }

    private void OnDestroy()
    {
        everything.Dispose();
    }

    private void OnApplicationQuit()
    {
        everything.StopService();
    }
}
