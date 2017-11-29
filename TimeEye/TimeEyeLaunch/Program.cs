using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TimeEye;

namespace TimeEyeLaunch
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputFile = string.Format("Period-{0}.log", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            string fileName = "2.log";
            string pattern = @"\d\d-\d\d \d\d:\d\d:\d\d,\d\d\d";
            string pattern2 = "MM-dd HH:mm:ss,fff";
            if (!File.Exists(fileName))
            {
                return;
            }
            Regex regex = new Regex(pattern);
            TimeSpan span = new TimeSpan(0, 0, 5);
            Core core = new Core();
        }
    }
}
