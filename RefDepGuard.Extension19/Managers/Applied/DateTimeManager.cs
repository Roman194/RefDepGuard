using System;

namespace RefDepGuard
{
    /// <summary>
    /// This class provides utility methods for managing date and time formatting.
    /// </summary>
    public class DateTimeManager
    {
        /// <summary>
        /// Gets the current date and time in a specific format: "dd.MM.yyyy-hh.mm.ss".
        /// </summary>
        /// <returns>The current DateTime value in a specific string format</returns>
        public static string GetCurrentDateTimeInRightFormat()
        {
            DateTime currentDateTime = DateTime.Now;

            return
                GetNumberWithFirstZeroIfNeeded(currentDateTime.Day) + "." +
                GetNumberWithFirstZeroIfNeeded(currentDateTime.Month) + "." +
                currentDateTime.Year + "-" +
                GetNumberWithFirstZeroIfNeeded(currentDateTime.Hour) + "." +
                GetNumberWithFirstZeroIfNeeded(currentDateTime.Minute) + "." +
                GetNumberWithFirstZeroIfNeeded(currentDateTime.Second);
        }

        /// <summary>
        /// Helper method to add a leading zero to a number if it is less than 10, to ensure consistent formatting in the date and time string.
        /// </summary>
        /// <param name="currentNumber">int value of the current num</param>
        /// <returns>A summary string with leading zero or without it</returns>
        private static string GetNumberWithFirstZeroIfNeeded(int currentNumber)
        {
            if (currentNumber < 10)
                return "0" + currentNumber;
            else
                return currentNumber.ToString();
        }
    }
}
