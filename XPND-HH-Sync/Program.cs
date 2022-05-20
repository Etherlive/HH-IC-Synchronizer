using System;

namespace XPND_HH_Sync
{
    public class Program
    {
        static Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();
        static Auth auth = XPND_HH_Sync.Auth.PMY;

        static void Main(string[] args)
        {
            string targetFile = args.Length == 0 ? "./export.csv" : args[0];
            DoSync(targetFile).Wait();
            Console.ReadLine();
        }

        static async Task DoSync(string targetFile)
        {
            if (!auth.LoadFromFile())
            {
                auth.PromptDetails();
            }

            while (!await Hire_Hop_Interface.Interface.Authentication.Login(cookie, auth.hh_email, auth.hh_pword))
            {
                Console.WriteLine("Login Failed. Please provide your credentails again.");
                auth.PromptDetails();
            }
            Console.WriteLine("HH Login Complete");

            auth.SaveDetails();

            Sync.LoadFileAndSync(cookie, targetFile);
        }
    }
}