using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TimeEye;

namespace TimeEyeLaunch
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = Console.ReadLine();
            Configuration config = new Configuration();
            config.LoadConfigFile();
            Core core = new Core(config);
            core.Scan(fileName);
            Console.WriteLine("**********Press any key to exit**********");
            Console.ReadKey();
        }
    }
}
