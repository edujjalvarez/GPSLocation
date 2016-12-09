using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace GPSLocation.Utils
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get { return CrossSettings.Current; }
        }

        private const string DesiredAccuracyKey = "desiredaccuracy_key";
        private static readonly int DesiredAccuracyDefault = 30;
        public static int DesiredAccuracy
        {
            get { return AppSettings.GetValueOrDefault<int>(DesiredAccuracyKey, DesiredAccuracyDefault); }
            set { AppSettings.AddOrUpdateValue<int>(DesiredAccuracyKey, value); }
        }

        private const string MaxSearchTimeInMinutesKey = "maxsearchtimeinminutes_key";
        private static readonly int MaxSearchTimeInMinutesDefault = 3;
        public static int MaxSearchTimeInMinutes
        {
            get { return AppSettings.GetValueOrDefault<int>(MaxSearchTimeInMinutesKey, MaxSearchTimeInMinutesDefault); }
            set { AppSettings.AddOrUpdateValue<int>(MaxSearchTimeInMinutesKey, value); }
        }

        private const string UpdateIntervalInSecondsKey = "updateintervalinseconds_key";
        private static readonly int UpdateIntervalInSecondsDefault = 10;
        public static int UpdateInterval
        {
            get { return AppSettings.GetValueOrDefault<int>(UpdateIntervalInSecondsKey, UpdateIntervalInSecondsDefault); }
            set { AppSettings.AddOrUpdateValue<int>(UpdateIntervalInSecondsKey, value); }
        }

    }
}
