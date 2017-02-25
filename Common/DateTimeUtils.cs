using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Puch.Common
{
    public static class DateTimeUtils
    {
        public static DateTime LastDayOfPreviousMonth
        {
            get
            {
                DateTime today = DateTime.Today;
                return new DateTime(today.Year, today.Month, 1).AddDays(-1);
            }
        }
        public static DateTime FirstDayOfPreviousMonth
        {
            get
            {
                DateTime today = DateTime.Today;
                return new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            }
        }
    }
}
