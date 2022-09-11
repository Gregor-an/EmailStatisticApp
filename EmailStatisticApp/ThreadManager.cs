using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailStatisticApp
{
    class ThreadManager
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        bool isRunning;
        object locker = new object();
        int queueCurrentCount = 0;
        int maxQueueCount;
        int cycleIntervalMs;
        List<Timer> timers = new List<Timer>();

        public ThreadManager(int maxThreadCount = 10, int maxQueueCount = 10, int cycleIntervalMs = 1000, int? minThreadCount = null)
        {
            this.isRunning = false;
            this.maxQueueCount = maxQueueCount + minThreadCount ?? 0;
            this.cycleIntervalMs = cycleIntervalMs;

            int workerThreads, completionPortThread;
            ThreadPool.GetMinThreads(out workerThreads, out completionPortThread);

            if(minThreadCount.HasValue)
            {
                workerThreads = Math.Max(workerThreads, minThreadCount.Value);
                completionPortThread = Math.Max(completionPortThread, minThreadCount.Value);
                ThreadPool.SetMinThreads(workerThreads, completionPortThread);
            }

            this.maxQueueCount = maxQueueCount + workerThreads;
        }

        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        public void Start()
        {
            this.isRunning = true;
        }

        public void Pause()
        {
            this.isRunning = false;
        }

        public void Stop()
        {
            this.isRunning = false;

            foreach(var timer in timers)
            {
                timer.Dispose();
            }

            lock(locker)
            {
                while(queueCurrentCount > 0)
                {
                    Monitor.Wait(locker);
                }
            }
        }

        public bool NextCycle()
        {
            if(this.isRunning)
            {
                Thread.Sleep(this.cycleIntervalMs);
            }

            return this.isRunning;
        }

        public bool StartNewJob(Action jobAction)
        {
            lock(locker)
            {
                while(queueCurrentCount >= this.maxQueueCount)
                {
                    Monitor.Wait(locker);
                }

                if(!isRunning)
                {
                    return false;
                }

                queueCurrentCount++;
            }

            ThreadPool.QueueUserWorkItem(x =>
            {
                try
                {
                    jobAction();
                }
                catch(Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
                finally
                {
                    lock(locker)
                    {
                        this.queueCurrentCount--;
                        Monitor.Pulse(locker);
                    }
                }
            });

            return true;
        }
    }
}
