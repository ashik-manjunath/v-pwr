namespace VseriesControllerLibrary_V1
{
    public  class ChannelRead
    {
        #region Public Properties

        /// <summary>
        /// This holds the respective channels
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// This holds the number of sample colleted
        /// </summary>
        public int Sample { get; set; }
        #endregion

        #region Private members 

        /// <summary>
        /// This holds the ADC count 
        /// </summary>
        private readonly List<int> adcCount;

        /// <summary>
        /// This holds the Raw value from the hardware 
        /// </summary>
        private readonly List<ulong> rawCount;


        #endregion

        #region Constructor 
        /// <summary>
        /// Constructor
        /// </summary>
        public ChannelRead()
        {
            adcCount = new List<int>();
            rawCount = new List<ulong>();
        }
        #endregion

        #region Public Members 
        /// <summary>
        /// This function will add the ADC count
        /// </summary>
        /// <param name="count">ADC count</param>
        public void AddCount(int count)
        {
            adcCount.Add(count);
        }

        /// <summary>
        /// This function will add the Raw ADC count
        /// </summary>
        /// <param name="count">Raw ADC count</param>
        public void AddRawCount(ulong count)
        {
            rawCount.Add(count);
        }

        /// <summary>
        /// This function will fetch the max count in the ADC list
        /// </summary>
        /// <returns>Int ADC count</returns>
        public int GetMaxCount()
        {

            if (!(adcCount.Count > 0))
            {
                return 0;
            }
            return adcCount.Max();
        }

        /// <summary>
        /// This function will get the max repeated ADC count in the list
        /// </summary>
        /// <returns>Int - Max repeated count</returns>
        public int GetMaxRepeatedCount()
        {
            if (!(adcCount.Count > 0))
            {
                return 0;
            }

            return GetMaxRepeatedCount(out int maxCount);
        }

        /// <summary>
        /// This fucntion will get the max repeated ADC count and number of times repeated
        /// </summary>
        /// <param name="max_count">Number of times max count repeated</param>
        /// <returns>Int- Max repeated count</returns>
        public int GetMaxRepeatedCount(out int max_count)
        {
            max_count = 1;
            if (!(adcCount.Count > 0))
            {
                return 0;
            }

            int[] arr = adcCount.ToArray();
            // Sort the array
            Array.Sort(arr);

            // find the max frequency using
            // linear traversal
            int res = arr[0];
            int curr_count = 1;

            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] == arr[i - 1])
                    curr_count++;
                else
                {
                    if (curr_count > max_count)
                    {
                        max_count = curr_count;
                        res = arr[i - 1];
                    }
                    curr_count = 1;
                }
            }

            // If last element is most frequent
            if (curr_count > max_count)
            {
                max_count = curr_count;
                res = arr[arr.Length - 1];
            }

            return res;

        }

        /// <summary>
        /// This function will get the min ADC count from the ADC list 
        /// </summary>
        /// <returns>Int - ADC Min count</returns>
        public int GetMinCount()
        {
            if (!(adcCount.Count > 0))
            {
                return 0;
            }

            return adcCount.Min();
        }

        /// <summary>
        /// This function will get the average ADC count from the list
        /// </summary>
        /// <returns>Int - Average count</returns>
        public int GetAvgCount()
        {
            if (!(adcCount.Count > 0))
            {
                return 0;
            }

            return (int)adcCount.Average();
        }

        /// <summary>
        /// This function will get the difference of Max and Min ADC count
        /// </summary>
        /// <returns>Int - Difference value of Max and Min ADC count</returns>
        public int GetDifference()
        {
            if (!(adcCount.Count > 0))
            {
                return 0;
            }

            return GetMaxCount() - GetMinCount();
        }

        /// <summary>
        /// This will string value of this class
        /// </summary>
        /// <returns>string value</returns>
        public override string ToString()
        {
            return $"Channel : {Channel}" +
                                $"\nMax count : {GetMaxCount()}" +
                                $"\nMin count : {GetMinCount()}" +
                                $"\nAverage count : {GetAvgCount()}" +
                                $"\nCount differance : {GetDifference()}" +
                                $"\nMax repeated count : {GetMaxRepeatedCount(out int maxRepeatedCount)}" +
                                $"\nMax repeated times : {maxRepeatedCount}" +
                                $"\nSamples : {Sample}";
        }
        #endregion
    }
}
