using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Common
{
    public class SingleThreadedBackgroundWorkProcessor<TInput> : IDisposable
    {
        private readonly BlockingCollection<TInput> inputQueue;
        private readonly Action<TInput, SingleThreadedBackgroundWorkProcessor<TInput>> processor;
        public SingleThreadedBackgroundWorkProcessor(Action<TInput, SingleThreadedBackgroundWorkProcessor<TInput>> processor, Action<Exception> errorHandler = null, int threadIdleTimeoutMs = 500, int maxWorkQueue = -1)
        {
            this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
            this.threadIdleTimeoutMs = Math.Max(threadIdleTimeoutMs, 0);
            this.errorHandler = errorHandler;

            inputQueue = maxWorkQueue < 1 ? new BlockingCollection<TInput>() : new BlockingCollection<TInput>(maxWorkQueue);
        }
        public SingleThreadedBackgroundWorkProcessor(Action<TInput> processor, Action<Exception> errorHandler = null, int threadIdleTimeoutMs = 500, int maxWorkQueue = -1)
            : this(processor is null ? throw new ArgumentNullException(nameof(processor)) : (item, _) => processor(item), errorHandler, threadIdleTimeoutMs, maxWorkQueue) { }

        private readonly Action<Exception> errorHandler;
        private int workerRunning;
        private int disposed;

        public int InputQueueCount => inputQueue.Count;

        public bool TryEnqueueInput(TInput item)
        {
            if (Volatile.Read(ref disposed) != 0 || inputQueue.IsAddingCompleted) return false;

            try
            {
                if (!inputQueue.TryAdd(item)) return false;

                TryStartWorker();
                return true;
            }
            catch (ObjectDisposedException) { return false; }
            catch (InvalidOperationException) { return false; }
        }
        public void ProcessInLine(TInput item) => processor(item, this);

        private volatile int threadIdleTimeoutMs;
        public int ThreadIdleTimeoutMs { get => threadIdleTimeoutMs; set => threadIdleTimeoutMs = value; }

        private void TryStartWorker()
        {
            if (ReserveWorker())
            {
                if (threadIdleTimeoutMs > 500)
                    Task.Factory.StartNew(WorkerLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                else Task.Run(WorkerLoop);
            }
        }
        private bool ReserveWorker()
        {
            if (Volatile.Read(ref workerRunning) != 0) return false;
            return Interlocked.CompareExchange(ref workerRunning, 1, 0) == 0;
        }
        private void WorkerLoop()
        {
            try
            {
                var localProcessor = processor;
                while (true)
                {
                    if (!inputQueue.TryTake(out var item, threadIdleTimeoutMs)) break;

                    try
                    {
                        localProcessor(item, this);
                    }
                    catch (Exception exception)
                    {
                        try { errorHandler?.Invoke(exception); } catch { }
                    }
                }
            }
            catch (ObjectDisposedException) { } //queue completed or disposed
            finally
            {
                if (Interlocked.CompareExchange(ref workerRunning, 0, 1) == 1)
                {
                    if (inputQueue.Count > 0) TryStartWorker();
                }
                if (Volatile.Read(ref disposed) != 0)
                {
                    try { inputQueue.Dispose(); } catch { }
                }
            }
        }

        public void Complete()
        {
            if (Volatile.Read(ref disposed) != 0 || inputQueue.IsAddingCompleted) return;
            try { inputQueue.CompleteAdding(); } catch { }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) != 0) return;

            if (!inputQueue.IsAddingCompleted) try { inputQueue.CompleteAdding(); } catch { }
            if (Volatile.Read(ref workerRunning) == 0)
            {
                try { inputQueue.Dispose(); } catch { }
            }
        }
        public bool ForceDispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) != 0) return false;

            if (!inputQueue.IsAddingCompleted) try { inputQueue.CompleteAdding(); } catch { }
            try { inputQueue.Dispose(); } catch { }
            return true;
        }
    }

    public class BackgroundWorkProcessor<TInput> : IDisposable
    {
        private readonly BlockingCollection<TInput> inputQueue;
        private readonly Action<TInput, BackgroundWorkProcessor<TInput>> processor;
        public BackgroundWorkProcessor(Action<TInput, BackgroundWorkProcessor<TInput>> processor, Action<Exception> errorHandler = null, int maxWorkerThreads = 32, int threadIdleTimeoutMs = 500, int maxWorkQueue = -1)
        {
            this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
            this.maxWorkerThreads = Math.Max(maxWorkerThreads, 1);
            this.threadIdleTimeoutMs = Math.Max(threadIdleTimeoutMs, 0);
            this.errorHandler = errorHandler;

            inputQueue = maxWorkQueue < 1 ? new BlockingCollection<TInput>() : new BlockingCollection<TInput>(maxWorkQueue);
        }
        public BackgroundWorkProcessor(Action<TInput> processor, Action<Exception> errorHandler = null, int maxWorkerThreads = 32, int threadIdleTimeoutMs = 500, int maxWorkQueue = -1)
            : this(processor is null ? throw new ArgumentNullException(nameof(processor)) : (item, _) => processor(item), errorHandler, maxWorkerThreads, threadIdleTimeoutMs, maxWorkQueue) { }

        private readonly Action<Exception> errorHandler;
        private int maxWorkerThreads;
        private int activeWorkersCount;
        private int disposed;

        public int InputQueueCount => inputQueue.Count;
        public int ActiveWorkers => Volatile.Read(ref activeWorkersCount);

        public bool TryEnqueueInput(TInput item)
        {
            if (Volatile.Read(ref disposed) != 0 || inputQueue.IsAddingCompleted) return false;

            try
            {
                if (!inputQueue.TryAdd(item)) return false;

                TryStartWorker();
                return true;
            }
            catch (ObjectDisposedException) { return false; }
            catch (InvalidOperationException) { return false; }
        }
        public void ProcessInLine(TInput item) => processor(item, this);

        private volatile int threadIdleTimeoutMs;
        public int ThreadIdleTimeoutMs { get => threadIdleTimeoutMs; set => threadIdleTimeoutMs = value; }
        public bool IncreaseWorkersCount(int increaseValue)
        {
            if (increaseValue < 1) return false;

            while (true)
            {
                var current = Volatile.Read(ref maxWorkerThreads);
                if (Interlocked.CompareExchange(ref maxWorkerThreads, current + increaseValue, current) == current) return true; ;
            }
        }
        public bool DecreaseWorkersCount(int decreaseValue)
        {
            if (decreaseValue < 1) return false;
            while (true)
            {
                var current = Volatile.Read(ref maxWorkerThreads);
                if (current - decreaseValue < 1) return false;
                if (Interlocked.CompareExchange(ref maxWorkerThreads, current - decreaseValue, current) == current) return true;
            }
        }

        private void TryStartWorker()
        {
            if (ReserveWorker())
            {
                if (threadIdleTimeoutMs > 500)
                    Task.Factory.StartNew(WorkerLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                else Task.Run(WorkerLoop);
            }
        }
        private bool ReserveWorker()
        {
            while (true)
            {
                int current = Volatile.Read(ref activeWorkersCount);
                int max = Volatile.Read(ref maxWorkerThreads);

                if (current >= max) return false;
                if (Interlocked.CompareExchange(ref activeWorkersCount, current + 1, current) == current) return true;
            }
        }
        private void WorkerLoop()
        {
            try
            {
                var localProcessor = processor;
                while (true)
                {
                    if (!inputQueue.TryTake(out var item, threadIdleTimeoutMs)) break;

                    try
                    {
                        localProcessor(item, this);
                    }
                    catch (Exception exception)
                    {
                        try { errorHandler?.Invoke(exception); } catch { }
                    }
                }
            }
            catch (ObjectDisposedException) { } //queue completed or disposed
            finally
            {
                if (Interlocked.Decrement(ref activeWorkersCount) == 0 && Volatile.Read(ref disposed) != 0)
                {
                    try { inputQueue.Dispose(); } catch { }
                }
            }
        }

        public void Complete()
        {
            if (Volatile.Read(ref disposed) != 0 || inputQueue.IsAddingCompleted) return;
            try { inputQueue.CompleteAdding(); } catch { }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed != 0) return;
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;

            if (!inputQueue.IsAddingCompleted) try { inputQueue.CompleteAdding(); } catch { }
            if (Volatile.Read(ref activeWorkersCount) == 0)
            {
                try { inputQueue.Dispose(); } catch { }
            }
        }
        public bool ForceDispose()
        {
            if (disposed != 0) return false;
            if (Interlocked.Exchange(ref disposed, 1) != 0) return false;

            if (!inputQueue.IsAddingCompleted) try { inputQueue.CompleteAdding(); } catch { }
            try { inputQueue.Dispose(); } catch { }
            return true;
        }
    }

    public class BackgroundWorkProcessor<TInput, TResult> : IDisposable
    {
        private readonly BlockingCollection<TInput> inputQueue;
        private readonly BlockingCollection<TResult> resultQueue;
        private readonly Func<TInput, BackgroundWorkProcessor<TInput, TResult>, TResult> processor;
        public BackgroundWorkProcessor(Func<TInput, BackgroundWorkProcessor<TInput, TResult>, TResult> processor, Action<Exception> errorHandler = null, int maxWorkerThreads = 32, int threadIdleTimeoutMs = 500, int maxWorkQueue = -1)
        {
            this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
            this.maxWorkerThreads = Math.Max(maxWorkerThreads, 1);
            this.threadIdleTimeoutMs = Math.Max(threadIdleTimeoutMs, 0);
            this.errorHandler = errorHandler;

            inputQueue = maxWorkQueue < 1 ? new BlockingCollection<TInput>() : new BlockingCollection<TInput>(maxWorkQueue);
            resultQueue = new BlockingCollection<TResult>();
        }
        public BackgroundWorkProcessor(Func<TInput, TResult> processor, Action<Exception> errorHandler = null, int maxWorkerThreads = 32, int threadIdleTimeoutMs = 500, int maxWorkQueue = -1)
            : this(processor is null ? throw new ArgumentNullException(nameof(processor)) : (item, _) => processor(item), errorHandler, maxWorkerThreads, threadIdleTimeoutMs, maxWorkQueue) { }

        private readonly Action<Exception> errorHandler;
        private int maxWorkerThreads;
        private int activeWorkersCount;
        private int disposed;

        public int InputQueueCount => inputQueue.Count;
        public int ResultQueueCount => resultQueue.Count;
        public int ActiveWorkers => Volatile.Read(ref activeWorkersCount);

        public bool TryEnqueueInput(TInput item)
        {
            if (Volatile.Read(ref disposed) != 0 || inputQueue.IsAddingCompleted) return false;

            try
            {
                if (!inputQueue.TryAdd(item)) return false;

                TryStartWorker();
                return true;
            }
            catch (ObjectDisposedException) { return false; }
            catch (InvalidOperationException) { return false; }
        }
        public bool TryDequeueResult(out TResult result)
        {
            try { return resultQueue.TryTake(out result); }
            catch (ObjectDisposedException) { result = default; return false; }
            catch (InvalidOperationException) { result = default; return false; }
        }
        public TResult ProcessInLine(TInput item) => processor(item, this);

        private volatile int threadIdleTimeoutMs;
        public int ThreadIdleTimeoutMs { get => threadIdleTimeoutMs; set => threadIdleTimeoutMs = value; }
        public bool IncreaseWorkersCount(int increaseValue)
        {
            if (increaseValue < 1) return false;

            while (true)
            {
                var current = Volatile.Read(ref maxWorkerThreads);
                if (Interlocked.CompareExchange(ref maxWorkerThreads, current + increaseValue, current) == current) return true; ;
            }
        }
        public bool DecreaseWorkersCount(int decreaseValue)
        {
            if (decreaseValue < 1) return false;
            while (true)
            {
                var current = Volatile.Read(ref maxWorkerThreads);
                if (current - decreaseValue < 1) return false;
                if (Interlocked.CompareExchange(ref maxWorkerThreads, current - decreaseValue, current) == current) return true;
            }
        }

        private void TryStartWorker()
        {
            if (ReserveWorker())
            {
                if (threadIdleTimeoutMs > 500)
                    Task.Factory.StartNew(WorkerLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                else Task.Run(WorkerLoop);
            }
        }
        private bool ReserveWorker()
        {
            while (true)
            {
                int current = Volatile.Read(ref activeWorkersCount);
                int max = Volatile.Read(ref maxWorkerThreads);

                if (current >= max) return false;
                if (Interlocked.CompareExchange(ref activeWorkersCount, current + 1, current) == current) return true;
            }
        }
        private void WorkerLoop()
        {
            try
            {
                var localProcessor = processor;
                while (true)
                {
                    if (!inputQueue.TryTake(out var item, threadIdleTimeoutMs)) break;

                    try
                    {
                        resultQueue.Add(localProcessor(item, this));
                    }
                    catch (Exception exception)
                    {
                        try { errorHandler?.Invoke(exception); } catch { }
                    }
                }
            }
            catch (ObjectDisposedException) { } //queue completed or disposed
            finally
            {
                if (Interlocked.Decrement(ref activeWorkersCount) == 0 && Volatile.Read(ref disposed) != 0)
                {
                    try { inputQueue.Dispose(); } catch { }
                    try { resultQueue.Dispose(); } catch { }
                }
            }
        }

        public void Complete()
        {
            if (Volatile.Read(ref disposed) != 0 || inputQueue.IsAddingCompleted) return;
            try { inputQueue.CompleteAdding(); } catch { }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;

            if (!inputQueue.IsAddingCompleted) try { inputQueue.CompleteAdding(); } catch { }
            if (Volatile.Read(ref activeWorkersCount) == 0)
            {
                try { inputQueue.Dispose(); } catch { }
                try { resultQueue.Dispose(); } catch { }
            }
        }
        public void ForceDispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;

            if (!inputQueue.IsAddingCompleted) try { inputQueue.CompleteAdding(); } catch { }

            try { inputQueue.Dispose(); } catch { }
            try { resultQueue.Dispose(); } catch { }
        }
    }
}
