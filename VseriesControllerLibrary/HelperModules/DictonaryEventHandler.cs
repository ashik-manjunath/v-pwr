
namespace VseriesControllerLibrary_V1
{

    /// <summary>
    /// This method gets executed in response to a USB Device connected or disconnected event that occurs in an host PC.
    /// </summary>
    public static class DictonaryEventHandler
    {
        #region Public Members
        /// <summary>
        /// This executed in response to a V-Up controller unit connected event that occurs in an host PC.
        /// </summary>  
        public static event EventHandler Controller_DeviceAdded;

        /// <summary>
        /// This executed in response to a V-Up controller unit disconnected event that occurs in an host PC.
        /// </summary>
        public static event EventHandler Controller_DeviceRemoved;

        /// <summary>
        ///  This executed in response to a USB Loopback Device connected event that occurs in an host PC.
        /// </summary>
        public static event EventHandler Loopback_DeviceAdded;

        /// <summary>
        /// This executed in response to a USB Loopback Device disconnected event that occurs in an host PC.
        /// </summary>
        public static event EventHandler Loopback_DeviceRemoved;

        #endregion

        #region Internal Members
        internal static void OnController_DeviceAddedChanged(Object obj)
        {
            EventHandler eh = Controller_DeviceAdded;
            if (eh != null)
            {
                Controller_DeviceAdded.Invoke(obj, EventArgs.Empty);
            }
        }
        internal static void OnController_DeviceRemovedChanged(Object obj)
        {
            EventHandler eh = Controller_DeviceRemoved;
            if (eh != null)
            {
                Controller_DeviceRemoved.Invoke(obj, EventArgs.Empty);
            }
        }
        internal static void OnLoopback_DeviceAddedChanged(Object obj)
        {
            EventHandler eh = Loopback_DeviceAdded;
            if (eh != null)
            {
                Loopback_DeviceAdded.Invoke(obj, EventArgs.Empty);
            }
        }
        internal static void OnLoopback_DeviceRemovedChanged(Object obj)
        {
            EventHandler eh = Loopback_DeviceRemoved;
            if (eh != null)
            {
                Loopback_DeviceRemoved.Invoke(obj, EventArgs.Empty);
            }
        }
        #endregion
    }
}
