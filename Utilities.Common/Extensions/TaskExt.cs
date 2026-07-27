using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Utilities.Common.Extensions
{
    public static class TaskExt
    {
        public static async Task<TResult> DelayParallel<TResult>(this Task<TResult> BaseTask, int milliSecond)
        {
            await Task.WhenAll(Task.Delay(milliSecond), BaseTask);
            return BaseTask.Result;
        }
        public static Task DelayParallel(this Task BaseTask, int milliSecond)
        {
            return Task.WhenAll(Task.Delay(milliSecond), BaseTask);
        }

        public static async Task<TResult> DelayBefore<TResult>(this Task<TResult> BaseTask, int milliSecond)
        {
            await Task.Delay(milliSecond);
            await BaseTask;
            return BaseTask.Result;
        }
        public static async Task DelayBefore(this Task BaseTask, int milliSecond)
        {
            await Task.Delay(milliSecond);
            await BaseTask;
        }

        public static async Task<TResult> DelayAfter<TResult>(this Task<TResult> BaseTask, int milliSecond)
        {
            await BaseTask;
            await Task.Delay(milliSecond);
            return BaseTask.Result;
        }
        public static async Task DelayAfter(this Task BaseTask, int milliSecond)
        {
            await BaseTask;
            await Task.Delay(milliSecond);
        }

        public static async Task<Tuple<T1, T2, T3>> WhenAll<T1, T2, T3>(Task<T1> Task1, Task<T2> Task2, Task<T3> Task3, params Task[] Tasks)
        {
            await Task.WhenAll(new List<Task>() { Task1, Task2, Task3 }.Concat(Tasks));
            return new Tuple<T1, T2, T3>(Task1.Result, Task2.Result, Task3.Result);
        }
        public static async Task<T1> WhenAll<T1>(Task<T1> Task1, params Task[] Tasks)
        {
            await Task.WhenAll(new List<Task>() { Task1, }.Concat(Tasks));
            return Task1.Result;
        }
        public static async Task<Tuple<T1, T2>> WhenAll<T1, T2>(Task<T1> Task1, Task<T2> Task2, params Task[] Tasks)
        {
            await Task.WhenAll(new List<Task>() { Task1, Task2 }.Concat(Tasks));
            return new Tuple<T1, T2>(Task1.Result, Task2.Result);
        }

        public static void Run(Action action, Action<Exception> logger)
        {
            Task.Run(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    logger(ex);
                }
            });
        }
        public static void Run<TResult>(Func<TResult> function, Action<Exception> logger)
        {
            Task.Run(() =>
            {
                try
                {
                    function.Invoke();
                }
                catch (Exception ex)
                {
                    logger(ex);
                }
            });
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function, Func<Exception, TResult> logger)
        {
            return Task.Run(() =>
            {
                try
                {
                   return function.Invoke();
                }
                catch (Exception ex)
                {
                    return logger.Invoke(ex);
                }
            });
        }
        public static Task<TResult> Run<TResult>(Func<TResult> function, Action<Exception> logger, TResult returnValueIfException)
        {
            return Task.Run(() =>
            {
                try
                {
                    return function.Invoke();
                }
                catch (Exception ex)
                {
                    logger.Invoke(ex);
                    return returnValueIfException;
                }
            });
        }
    }
}
