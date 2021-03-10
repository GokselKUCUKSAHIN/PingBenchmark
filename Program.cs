using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PingBenchmark
{
    class Program
    {
        static string pwd = String.Empty;
        private static readonly string regEx = @"Reply from [0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}: bytes=[0-9]+ time=([0-9]+)ms TTL=[0-9]+";
        private static readonly string ipRex = @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$";
        private static readonly string DEFAULT_ADDRESS = "192.168.1.1";
        private static readonly int DEFAULT_TIME = 10000;

        static void Main(string[] args)
        {
            string time = "10";
            string address = DEFAULT_ADDRESS;
            if (args.Length >= 1)
            {
                // Get adrress
                address = args[0];
                if (args.Length >= 2)
                {
                    // Get time
                    time = args[1];
                }
            }
            int milis = 0;
            try
            {
                milis = int.Parse(time);
                milis *= 1000;
            }
            catch
            {
                milis = DEFAULT_TIME;
            }
            // Command done.
            string cmdOut = Ping(IsValidAddress(address) ? address : DEFAULT_ADDRESS, milis);
            Console.WriteLine(cmdOut);
            var parsed = ParseOutput(cmdOut);
            int[] numbers = GetNumberArray(parsed);
            double avg = GetAveragePing(numbers);
            double sd = GetStandarDeviation(numbers, avg);
            Console.WriteLine("Ortalama = {0:0.0000}", avg);
            Console.WriteLine("Standart Sapma = {0:0.0000}", sd);
            Console.ReadKey();
        }

        private static bool IsValidAddress(string adr)
        {
            return Regex.IsMatch(adr, ipRex);
        }

        private static int[] GetNumberArray(string[] rows)
        {
            List<int> numberList = new List<int>();
            foreach (string item in rows)
            {
                string match = ShowMatch(item, regEx);
                if (!match.Equals(String.Empty))
                {
                    numberList.Add(int.Parse(match));
                }
            }
            return numberList.ToArray();
        }

        private static double GetAveragePing(int[] numbers)
        {
            if(numbers.Length >= 1)
            {
                double sum = 0;
                foreach (int number in numbers)
                {
                    sum += number;
                }
                return sum / numbers.Length;
            }
            return 0;
        }

        private static double GetStandarDeviation(int[] numbers)
        {
            return GetStandarDeviation(numbers, GetAveragePing(numbers));
        }

        private static double GetStandarDeviation(int[] numbers, double mean)
        {
            if(numbers.Length >=  1)
            {
                double sumSqr = 0;
                foreach (int number in numbers)
                {
                    sumSqr += Math.Pow(number - mean, 2);
                }
                return Math.Sqrt(sumSqr / numbers.Length);
            }
            return 0;
        }

        private static string Ping(string ip, int millis)
        {
            pwd = Directory.GetCurrentDirectory(); // Get Current Working Directory as String.
            if (pwd.Equals(String.Empty))
            {
                throw new DirectoryNotFoundException();
            }
            // Create Command
            var processInfo = new ProcessStartInfo("cmd.exe", String.Format("/c ping -t {0}", ip))
            {
                // /c is require. Some how idk
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = pwd
            };
            StringBuilder sb = new StringBuilder(); // Create Process
            Process p = Process.Start(processInfo); // RUN COMMAND
            p.OutputDataReceived += (sender, pArgs) => sb.AppendLine(pArgs.Data);
            p.BeginOutputReadLine(); // Read the output.
            Console.Clear();
            for (int i = millis / 1000; i > 0; i--)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("                                                      ");
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("{0} saniye kaldı...", i);
                System.Threading.Thread.Sleep(1000);
            }
            Console.Clear();
            p.Kill();
            return sb.ToString();
        }

        private static string ShowMatch(string text, string expr)
        {
            MatchCollection mc = Regex.Matches(text, expr);
            string newText = text;
            foreach (Match m in mc)
            {
                return m.Groups[1].ToString();
            }
            return String.Empty;
        }

        private static string[] ParseOutput(string dumpedProc)
        {
            return dumpedProc.Split('\n');
        }
    }
}
