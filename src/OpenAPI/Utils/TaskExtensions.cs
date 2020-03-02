using System;
using System.Threading.Tasks;

namespace OpenAPI.Utils
{
    public static class TaskExtensions
    {
       /* public static Task Then(this Task task, Action<Task> continuationAction)
        {
            return task.ContinueWith(x => continuationAction);
        }*/
        
        public static Task<TResult> Then<TResult>(this Task<TResult> task, Action<TResult> continuationAction)
        {
            return task.ContinueWith(x =>
            {
                continuationAction(x.Result);

                return x.Result;
            });
        }
    }
}