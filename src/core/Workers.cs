using System;
using System.Threading;
using System.Threading.Tasks;

namespace TRS2
{
    // Shared background-task startup and exception propagation.
    // Not specific to SPC, TimeTagger, steppers, or trimmers.
    public static class Workers
    {
        private static readonly object sync = new();
        private static Task[] tasks = [];

        // NEW: Call once at the start of a new measurement.
        // All tasks from the previous measurement must have stopped.
        public static void Reset()
        {
            lock (sync)
            {
                foreach (Task task in tasks)
                {
                    if (!task.IsCompleted)
                        throw new InvalidOperationException(
                            "A background task from the previous measurement is still running.");
                }

                Volatile.Write(ref tasks, Array.Empty<Task>());
            }
        }

        // a long running task that generates an indepenedet thread
        public static Task StartLongRunning(Action action)
        {
            Task task = Task.Factory.StartNew(action,CancellationToken.None,TaskCreationOptions.LongRunning,TaskScheduler.Default);

            lock (sync)
            {
                Task[] previous = Volatile.Read(ref tasks);
                int keep = 0;

                // Discard successful completed tasks to avoid growing indefinitely.
                foreach (Task old in previous)
                    if (!old.IsCompletedSuccessfully)
                        keep++;

                Task[] updated = new Task[keep + 1];
                int i = 0;
                foreach (Task old in previous)
                    if (!old.IsCompletedSuccessfully)
                        updated[i++] = old;

                updated[i] = task;
                Volatile.Write(ref tasks, updated);
            }
            return task;
        }

        // Call on the measurement/consumer thread to rethrow worker errors.
        // This method does not create or wait for any new thread.
        public static void ThrowIfFailed()
        {
            foreach (Task task in Volatile.Read(ref tasks))
            {
                if (task.IsFaulted || task.IsCanceled)
                    task.GetAwaiter().GetResult();
            }
        }
    }
}
