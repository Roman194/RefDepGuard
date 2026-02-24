using System;

namespace RefDepGuard
{
    public class DateTimeManager
    {
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

        private static string GetNumberWithFirstZeroIfNeeded(int currentNumber)
        {
            if (currentNumber < 10)
                return "0" + currentNumber;
            else
                return currentNumber.ToString();
        }
    }
}
