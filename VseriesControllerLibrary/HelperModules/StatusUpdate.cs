using System;
using System.Collections.Generic;

namespace VseriesControllerLibrary_V1.HelperModules
{
    public class StatusUpdate
    {

        #region Private properties 
        private bool triggerStatus = true;
        private readonly object methodLockInput = new object();
        private readonly object methodLockOutput = new object();
        #endregion

        #region Public Event handler
        /// <summary>
        /// This executed in response to a V-Up controller unit connected event that occurs in an host PC.
        /// </summary>  
        public event EventHandler StatusUpdateEventHandle;
        #endregion

        #region Internal Members
        internal void OnStatusUpdateEventHandleChanged(Object obj)
        {
            EventHandler eh = StatusUpdateEventHandle;
            if (eh != null)
            {
                StatusUpdateEventHandle.Invoke(obj, EventArgs.Empty);
            }
        }
        #endregion

        #region Private Members 
        private readonly Queue<string> outPut;
        #endregion

        #region Constructor
        public StatusUpdate()
        {
            outPut = new Queue<string>();
        }
        #endregion

        #region Public Module 
        internal void Add(string message, int queueLength = 1000)
        {
            lock (methodLockInput)
            {
                if (queueLength < Count())
                {
                    outPut.Dequeue();
                }
                if (Count() == 0)
                {
                    triggerStatus = true;
                }

                outPut.Enqueue($"{DateTime.Now:HH:mm:ss.fff} : {message}");
                Sleep(10);
                if (triggerStatus)
                {
                    OnStatusUpdateEventHandleChanged(this);
                    triggerStatus = false;
                }
            }
        }
        public string Get()
        {
            string retValue = "";
            lock (methodLockOutput)
            {
                retValue = outPut.Dequeue();
                if (Count() == 0)
                {
                    triggerStatus = true;
                }
            }
            return retValue;
        }
        public void Clear()
        {
            outPut.Clear();
            triggerStatus = true;
        }
        public int Count()
        {
            return outPut.Count;
        }

        #endregion

        #region Private Module 
        private void Sleep(int waitTimeMilli)
        {
            if (waitTimeMilli <= 0)
                waitTimeMilli = 1;

            int i = 0;
            System.Timers.Timer delayTimer = new System.Timers.Timer(waitTimeMilli)
            {
                AutoReset = false //so that it only calls the method once
            };
            delayTimer.Elapsed += (s, args) => i = 1;
            delayTimer.Start();
            while (i == 0) { };
        }
        #endregion
    }
}
