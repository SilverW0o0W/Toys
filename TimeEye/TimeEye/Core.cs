using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TimeEye
{
    public class Core
    {
        public Configuration Config { get; set; }

        public Core(Configuration config)
        {
            this.Config = config;
        }

        public void Scan(string inputFile, string pattern = null, string format = null)
        {
            if (!File.Exists(inputFile))
            {
                return;
            }
            Regex regex = new Regex(pattern);
            using (StreamReader reader = new StreamReader(inputFile, Encoding.UTF8))
            {
                DateTime lastTime = DateTime.MinValue;
                string lastLine = string.Empty;
                bool firstMatch = true;
                bool lastPrinted = false;

                using (StreamWriter writer = new StreamWriter(Config.OutputFile))
                {
                    while (true)
                    {
                        string currentLine = reader.ReadLine();
                        if (currentLine == null)
                        {
                            break;
                        }
                        MatchCollection collection = regex.Matches(currentLine);
                        if (collection.Count > 0)
                        {
                            DateTime currentTime = ConvertToTime(collection[0].Value, format);
                            if (currentTime == DateTime.MinValue)
                            {
                                continue;
                            }
                            //first line
                            if (firstMatch)
                            {
                                firstMatch = false;
                            }
                            else if (Config.Period <= currentTime - lastTime)
                            {
                                //if last line was printed, print current line directly.
                                //else print last line and current line.
                                if (!lastPrinted)
                                {
                                    writer.WriteLine();
                                    writer.WriteLine(lastLine);
                                }
                                writer.WriteLine(currentLine);
                                lastPrinted = true;
                            }
                            else
                            {
                                //clear last line printed signal
                                lastPrinted = false;
                            }
                            lastTime = currentTime;
                            lastLine = currentLine;
                        }
                    }
                }
            }
        }

        private DateTime ConvertToTime(string strTime, string format)
        {
            DateTime time;
            if (!DateTime.TryParseExact(strTime, format, CultureInfo.CurrentCulture, DateTimeStyles.AllowInnerWhite, out time))
            {
                time = DateTime.MinValue;
            }
            return time;
        }

    }
}
