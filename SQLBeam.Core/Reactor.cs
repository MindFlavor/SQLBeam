using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SQLBeam.LoggingExtensions;

namespace SQLBeam.Core
{
    public class Reactor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Reactor));

        private Guid guid;

        private Thread mainThread;
        private Configuration configuration;
        private bool fAlive = false;

        private List<Thread> lThreadPool = null;
        private System.Threading.ManualResetEvent mreEventToProcess;
        private Database.ConfigDatabase configDatabase = null;
        private System.Collections.Concurrent.ConcurrentQueue<Tasks.ExecutableTask> qTasks = null;

        private static long RunningTasks = 0;

        public Reactor(Configuration configuration)
        {
            this.configuration = configuration;
            this.configDatabase = new Database.ConfigDatabase(this.configuration);

            this.guid = Guid.NewGuid();
            log.InfoFormat($"Reactor {guid.ToString():S} constructed");
        }

        public void Start()
        {
            log.DebugFormat($"Reactor core {guid.ToString():S} start requested");
            if ((mainThread != null) || (fAlive))
            {
                throw new Exceptions.BeamExeception();
            }

            mainThread = new Thread(new ThreadStart(Core))
            {
                Priority = ThreadPriority.BelowNormal
            };
            fAlive = true;
            log.Debug("Reactor core starting...");
            mainThread.Start();
        }

        public void Stop()
        {
            fAlive = false;
            if (!mainThread.Join(3000))
                mainThread.Abort();

            if (lThreadPool != null)
            {
                lThreadPool.AsParallel().ForAll(t =>
                {
                    if (!t.Join(3000))
                        t.Abort();
                });
            }

            mainThread = null;
        }

        protected void Core()
        {
            log.Debug("Reactor core started");

            qTasks = new System.Collections.Concurrent.ConcurrentQueue<Tasks.ExecutableTask>();
            mreEventToProcess = new ManualResetEvent(false);

            #region Allocate thread pool
            lThreadPool = new List<Thread>();
            for (int i = 0; i < configuration.MaximumThreads; i++)
            {
                var t = new Thread(new ParameterizedThreadStart(ExecutorThread))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                };
                t.Start(i);
                lThreadPool.Add(t);
            }
            #endregion

            Random r = new Random();

            while (fAlive)
            {
                // look for tasks to execute
                // schedule pending tasks as new threads                              
                // Schedule at most TotalThreads - RunningTasks tasks
                var tasksToSchedule = configuration.MaximumThreads - Interlocked.Read(ref RunningTasks);

                try
                {
                    var lExecutableTasks = configDatabase.ScheduleTasks(guid, tasksToSchedule);
                    if (lExecutableTasks.Count > 0)
                    {
                        log.InfoFormat("Scheduled {0:N0} tasks", lExecutableTasks.Count);
                        lExecutableTasks.ForEach(t => qTasks.Enqueue(t));
                        mreEventToProcess.Set();
                    }
                }
                catch (Exception exce)
                {
                    log.ErrorFormat($"Cannot schedule tasks: ${exce.ToString()}");
                    if (exce is System.Data.SqlClient.SqlException)
                    {
                        Thread.Sleep(3000); // sleep for 3 seconds to avoid polluting the log.
                    }
                }
                Thread.Sleep(configuration.MainPollIntervalMilliseconds);
            }
            log.Debug("Reactor core shutting down gracefully");
        }

        protected void ExecutorThread(object param)
        {
            try
            {
                log.DebugFormat("ExecutorThread started");
                while (fAlive)
                {
                    if (mreEventToProcess.WaitOne(300))
                    {
                        // processing to do
                        log.VerboseFormat("ExecutorThread awoken");

                        while (fAlive && qTasks.TryDequeue(out Tasks.ExecutableTask taskToExecute))
                        {
                            Interlocked.Increment(ref RunningTasks);

                            try
                            {
                                log.InfoFormat("Starting task {0:S}", taskToExecute.ToString());
                                configDatabase.MoveExecutableTaskToRunning(taskToExecute);
                                taskToExecute.Task.Execute(taskToExecute.Destination);
                                log.InfoFormat("Completed task {0:S}", taskToExecute.ToString());
                                configDatabase.MoveRunningTaskToCompleted(taskToExecute);
                            }
                            catch (ThreadInterruptedException tie)
                            {
                                log.ErrorFormat($"Thread interrupted from outside: {tie.ToString():S}");
                                throw tie;
                            }
                            catch (Exception exce)
                            {
                                log.WarnFormat($"Unhandled ExecutableTask exception: {exce.ToString():S}. Task will be logged as failed.");
                                // move task to failed tasks
                                configDatabase.MoveRunningTaskToError(taskToExecute, exce);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref RunningTasks);
                            }
                        }

                        log.VerboseFormat("ExecutorThread snoozing");
                        mreEventToProcess.Reset();
                    }
                }

                log.DebugFormat("ExecutorThread shutting down gracefully");
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExecutorThread shutting down due to exception: {0:S}", e.ToString());
            }
        }
    }


}
