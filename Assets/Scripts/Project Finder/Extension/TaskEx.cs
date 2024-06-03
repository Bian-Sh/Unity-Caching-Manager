using System.Collections;
using System.Threading.Tasks;

public static class TaskEx
{
    public static async Task WaitUntil(System.Func<bool> predicate)
    {
        if (null != predicate)
        {
            while (!predicate())
            {
                await Task.Yield();
            }
        }
    }
}
