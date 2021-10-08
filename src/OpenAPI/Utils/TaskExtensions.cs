using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAPI.Utils
{
    public static class TaskExtensions
    {
       /* public static Task Then(this Task task, Action<Task> continuationAction)
        {
            return task.ContinueWith(x => continuationAction);
        }*/
        
       public static Task<TResult> ThenAsync<TResult>(this Task<TResult> task, Action<TResult> continuationAction)
       {
           return task.ContinueWith(x =>
           {
               continuationAction(x.Result);

               return x.Result;
           });
       }
       
       /// <summary>
       ///  Blocking version of ContinueWith.
       /// </summary>
       /// <param name="task"></param>
       /// <param name="continuationAction"></param>
       /// <param name="cancellationToken"></param>
       /// <typeparam name="TResult"></typeparam>
       /// <returns></returns>
       public static TResult Then<TResult>(this Task<TResult> task, Action<TResult> continuationAction)
       {
           return Then<TResult>(task, continuationAction, CancellationToken.None);
       }
       
       /// <summary>
       ///  Blocking version of ContinueWith.
       /// </summary>
       /// <param name="task"></param>
       /// <param name="continuationAction"></param>
       /// <param name="cancellationToken"></param>
       /// <typeparam name="TResult"></typeparam>
       /// <returns></returns>
        public static TResult Then<TResult>(this Task<TResult> task, Action<TResult> continuationAction, CancellationToken cancellationToken)
        {
            var t = task.ContinueWith(x =>
            {
                continuationAction(x.Result);

                return x.Result;
            }, cancellationToken);

            t.Wait(cancellationToken);

            return t.Result;
        }
    }
}