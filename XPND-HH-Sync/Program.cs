using System;

namespace XPND_HH_Sync
{
    internal class Program
    {
        

        static void Main(string[] args)
        {
            string targetFile = args.Length == 0 ? "./export.csv" : args[0];

            if (File.Exists(targetFile))
            {
                string fileContent = File.ReadAllText(targetFile);
                var expenses = CSVParser.ParseFile(fileContent);
            }
            else
            {
                Console.WriteLine($"Cant Find File {targetFile}.\nTo submit your own please drag and drop the file onto the executable.\nOr place the export file into the same directory as the executable and name it 'export.csv'");
            }
        }
    }
}