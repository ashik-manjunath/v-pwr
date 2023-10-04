using System.Collections;

namespace VseriesControllerLibrary_V1.SysData
{
    internal class ChannelBaseClass
    {
        /// <summary>
        /// Sorted list contains all the calibration data
        /// </summary>
        public SortedList CalData { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="adcCount">ADC Count</param>
        /// <param name="resValue">ADC Value</param>
        /// <returns></returns>
        public bool AddCalData(int adcCount, double resValue)
        {
            bool retVal = true;
            CalData.Add(resValue, adcCount);
            return retVal;
        }

        /// <summary>
        /// Get the minium value from the dictionary
        /// </summary>
        /// <param name="adcCount">Minimum ADC count</param>
        /// <param name="resValue">Minimum ADC Value</param>
        /// <returns></returns>
        public bool GetMinValuePair(out int adcCount, out double resValue)
        {
            bool retVal = true;
            if (CalData.Count >= 1)
            {
                resValue = (double)CalData.GetKey(0);
                adcCount = (int)CalData.GetByIndex(0);
            }
            else
            {
                adcCount = 0;
                resValue = 0;
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Get the maximum value from the dictionary
        /// </summary>
        /// <param name="adcCount">Maximum ADC count</param>
        /// <param name="resValue">Maximum ADC value</param>
        /// <returns></returns>
        public bool GetMaxValuePair(out int adcCount, out double resValue)
        {
            bool retVal = true;
            if (CalData.Count >= 1)
            {
                resValue = (double)CalData.GetKey(CalData.Count - 1);
                adcCount = (int)CalData.GetByIndex(CalData.Count - 1);
            }
            else
            {
                adcCount = 0;
                resValue = 0;
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Get the middle value from the dictionary
        /// </summary>
        /// <param name="adcCount">Middle ADC count</param>
        /// <param name="resValue">Middle ADC value</param>
        /// <returns></returns>
        public bool GetMidValuePair(out int adcCount, out double resValue)
        {
            bool retVal = true;
            if (CalData.Count >= 1)
            {
                int MidIndex = Convert.ToInt32(CalData.Count / 2);
                resValue = (double)CalData.GetKey(MidIndex);
                adcCount = (int)CalData.GetByIndex(MidIndex);
            }
            else
            {
                adcCount = 0;
                resValue = 0;
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Get the index of the value
        /// </summary>
        /// <param name="ind">Integer index</param>
        /// <returns></returns>
        public double GetValueIndex(int ind)
        {
            double adcCount = (int)CalData.GetByIndex(ind);
            return adcCount;
        }
    }
}
