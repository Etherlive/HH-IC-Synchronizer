using System;

namespace XPND_HH_Sync
{
    internal class Program
    {
        static Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();
        static Auth auth = XPND_HH_Sync.Auth.PMY;

        static void Main(string[] args)
        {
            auth.LoadFromFile();

            string targetFile = args.Length == 0 ? "./export.csv" : args[0];

            if (File.Exists(targetFile))
            {
                try
                {
                    string fileContent = File.ReadAllText(targetFile);

                    var expenses = CSVParser.ParseFile(fileContent);

                    Sync(expenses).Wait();
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

            Console.ReadLine();
        }

        static async Task Sync(Expense[] expenses)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine($"Sync Started At {start.ToString("dd/MM/yyyy HH:mm:ss")}");

            if (!await Hire_Hop_Interface.Interface.Authentication.Login(cookie, auth.hh_email, auth.hh_pword))
            {
                Console.WriteLine("Login Failed. Please provide your credentails again.");
                auth.PromptDetails();
                await Sync(expenses);
                return;
            }
            Console.WriteLine("HH Login Complete");

            auth.SaveDetails();

            if (!await Hire_Hop_Interface.Interface.Authentication.ToggleAdmin(cookie))
            {
                await Hire_Hop_Interface.Interface.Authentication.ToggleAdmin(cookie);
            }

            ExpenseSync.Sync(expenses, cookie).Wait();

            Console.WriteLine($"Synced {ExpenseSync.expensesSynced} Expenses");

            DateTime end = DateTime.Now;
            TimeSpan dur = end - start;
            Console.WriteLine($"Sync Finished At {end.ToString("dd/MM/yyyy HH:mm:ss")} Taking {dur.TotalSeconds} Seconds");
        }
    }
}