using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSLocation.Utils
{
    public class Log
    {
        private static Log _instance;
        private Log()
        {
            
        }

        public static Log GetInstance()
        {
            return _instance ?? (_instance = new Log());
        }

        public void Print(string tag, string msg)
        {
            System.Diagnostics.Debug.WriteLine(tag + " >>> " + msg);
        }

        public void Print(string tag, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(tag + " >>> " + format, args);
        }
    }
}
