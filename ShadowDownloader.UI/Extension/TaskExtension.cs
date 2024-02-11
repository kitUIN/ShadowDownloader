using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShadowDownloader.UI.Extension;

public static class TaskExtension
{
    public static void StartAll(this IEnumerable<Task> tasks)
    {
        foreach (var task in tasks)
        {
            task.Start();
        }
    }
}