using System;

namespace Halmid_Server.Functions
{
    public class Convert_Days
    {
        public static string Convert(int duration_in_minutes, string expire)
        {
            string duration = String.Empty;

            switch (duration_in_minutes)
            {
                case 30:
                    duration = duration_in_minutes + " minutes (" + expire + ")";
                    break;
                case 60:
                case 120:
                case 300:
                case 720:
                    int hours = duration_in_minutes / 60;
                    duration = hours + " hours (" + expire + ")";
                    break;
                case 1440:
                case 2880:
                case 7200:
                    int days = duration_in_minutes / 1440;
                    duration = days + " days (" + expire + ")";
                    break;
                case 10080:
                case 20160:
                    int weeks = duration_in_minutes / 10080;
                    duration = weeks + " weeks (" + expire + ")";
                    break;
                default:
                    duration = "Permanent";
                    break;
            }
            return duration;
        }
    }
}
