using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace parcel_generator
{
    class Program
    {
        const char ColumnDelimiter = '\t';
        const int state_township_number = 5;
        const int state_assigned_district_number = 7;
        const int property_address = 9;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string fileName = args[0];
            Console.WriteLine("Reading from {0}...", fileName);

            string outputFileName = GenerateOutputFileName(fileName);
            Console.WriteLine("Output file will be {0}", outputFileName);

            File.Delete(outputFileName);

            using (StreamWriter writer = new StreamWriter(outputFileName))
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    SkipHeader(reader, writer);
                    ProcessFile(reader, writer);
                }
            }
        }

        private static void ProcessFile(StreamReader reader, StreamWriter writer)
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                List<string> columns = new List<string>(line.Split(new char[] { ColumnDelimiter }));

                string hashValue = GenerateHashFromRow(columns);

                if (!string.IsNullOrEmpty(hashValue)) // skip columns with bad data
                {
                    columns.Add(hashValue);
                    writer.WriteLine(ColumnsToLine(columns));
                }
            }
        }

        private static string ColumnsToLine(List<string> columns)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < columns.Count; ++i)
            {
                if (i + 1 == columns.Count)
                    sb.AppendFormat(columns[i]);
                else
                    sb.AppendFormat("{0}{1}", columns[i], ColumnDelimiter);
            }
            return sb.ToString();
        }

        public static string CalculateMD5Hash(string input)
        {
            byte[] hash = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(input));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; ++i)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private static string GenerateHashFromRow(List<string> columns)
        {
            if (IsRowDataBad(columns))
                return string.Empty;

            string concatinatedValue = string.Format("{0}_{1}_{2}", 
                columns[state_township_number],
                columns[state_assigned_district_number],
                columns[property_address].ToUpper());

            return CalculateMD5Hash(concatinatedValue);
        }

        private static bool IsRowDataBad(List<string> columns)
        {
            return string.IsNullOrEmpty(columns[state_township_number]) ||
                   string.IsNullOrEmpty(columns[state_assigned_district_number]) ||
                   string.IsNullOrEmpty(columns[property_address]);
        }

        private static void SkipHeader(StreamReader reader, StreamWriter writer)
        {
            writer.WriteLine(reader.ReadLine());
        }

        private static string GenerateOutputFileName(string inputFileName)
        {
            return Path.Combine(
                Path.GetDirectoryName(inputFileName),
                Path.GetFileNameWithoutExtension(inputFileName) + "_converted" + Path.GetExtension(inputFileName)
                );
        }

        private static void PrintUsage()
        {
            Console.WriteLine("This program generates a parcel number.");
            Console.WriteLine("Usage: {0} <datafile.tsv>", ExtractExeNameFromFullFilePath(Environment.GetCommandLineArgs()[0]));
        }

        private static string ExtractExeNameFromFullFilePath(string fullPath)
        {
            return Path.GetFileName(fullPath);
        }
    }
}
