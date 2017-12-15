using System;
using System.IO;
using TimeEye;

namespace TimeEyeLaunch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**********Input file name**********");
            string path = args.Length > 0 ? args[0] : Console.ReadLine();

            if (File.Exists(path))
            {
                Console.WriteLine("File path: {0}.", path);
            }
            else if (Directory.Exists(path))
            {
                Console.WriteLine("Folder path: {0}.", path);
            }
            else
            {
                Console.WriteLine("The path is invalid.");
                PrintEnd();
                return;
            }

            Configuration config = new Configuration();
            config.LoadConfigFile();
            Core core = new Core(config);
            core.ScanFile(path);
            PrintEnd();
        }

        static void PrintEnd()
        {
            Console.WriteLine("**********Press any key to exit**********");
            Console.ReadKey();
        }
    }
}
