using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPND_HH_Sync
{
    public static class Sync
    {
        public static void LoadFileAndSync(Hire_Hop_Interface.Interface.Connections.CookieConnection cookie, string targetFile)
        {
            if (File.Exists(targetFile))
            {
                try
                {
                    string fileContent = File.ReadAllText(targetFile);

                    var expenses = CSVParser.ParseFile(fileContent);

                    SyncExpenses(cookie,expenses).Wait();
                }
                catch (IOException e)
                {
                    Console.WriteLine($"Cant Read File! {e.ToString()}");
                }
            }
            else
            {
                Console.WriteLine($"Cant Find File {targetFile}.\nTo submit your own please drag and drop the file onto the executable.\nOr place the export file into the same directory as the executable and name it 'export.csv'");
            }
        }

        static async Task SyncExpenses(Hire_Hop_Interface.Interface.Connections.CookieConnection cookie, Expense[] expenses)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine($"Sync Started At {start.ToString("dd/MM/yyyy HH:mm:ss")}");

            if (!await Hire_Hop_Interface.Interface.Authentication.ToggleAdmin(cookie))
            {
                if (!await Hire_Hop_Interface.Interface.Authentication.ToggleAdmin(cookie))
                {
                    Console.WriteLine("Failed To Gain Administrator On HH. Continue Without? Y/N ");
                    if (!Console.ReadLine().ToLower().Contains("Y"))
                    {
                        return;
                    }
                }
            }

            ExpenseSync.Sync(expenses, cookie).Wait();

            Console.WriteLine($"Synced {ExpenseSync.expensesSynced} Expenses");

            DateTime end = DateTime.Now;
            TimeSpan dur = end - start;
            Console.WriteLine($"Sync Finished At {end.ToString("dd/MM/yyyy HH:mm:ss")} Taking {dur.TotalSeconds} Seconds");
        }
    }
}
