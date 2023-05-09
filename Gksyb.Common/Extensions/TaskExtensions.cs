using Chloe.Threading.Tasks;

namespace Gksyb.Common
{
    public static class TaskExtensions
    {
        public static TResult Result<TResult>(this Task<TResult> task)
        {
            return task.GetResult();
        }

        public static void Result(this Task task)
        {
            task.GetResult();
        }

        public static TResult Result<TResult>(this ValueTask<TResult> task)
        {
            return task.GetResult();
        }
    }
}