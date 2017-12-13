using FieldManager;
using System;
using System.Text.RegularExpressions;

namespace FieldExtractorCMD
{
    class Program
    {
        static void Main(string[] args)
        {
            Process(args);
            Console.ReadLine();
        }

        static void Process(string[] args)
        {
            FieldWorker worker;
            int[] report = new int[4];
            string[] inputArray;

            worker = new FieldWorker();

            if (args.Length == 0)
            {
                Console.WriteLine("Please drag a log file here.");
                string input;
                input = Console.ReadLine();
                inputArray = new string[] { input };
            }
            else
            {
                inputArray = args;
            }

            try
            {
                if (!worker.Initialize())
                {
                    Console.WriteLine("Initialize worker failed.");
                    return;
                }
                report = worker.Run(inputArray);
                Console.WriteLine("JobId:{0}", worker.JobId);
            }
            catch (HandlerException ex)
            {
                Console.WriteLine("Handler exception occurred. Reason: {0}.", ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred. Reason: {0}.", ex.ToString());
            }

            Console.WriteLine("Total: {0}", report[3]);
            Console.WriteLine("Success: {0}", report[0]);
            Console.WriteLine("Failed: {0}", report[1]);
            Console.WriteLine("Skipped: {0}", report[2]);
        }
    }
}
