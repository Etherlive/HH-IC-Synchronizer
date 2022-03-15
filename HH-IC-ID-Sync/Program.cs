// See https://aka.ms/new-console-template for more information

DateTime start = DateTime.Now;
Console.WriteLine($"Sync Started At {start.ToString("dd/MM/yyyy HH:mm:ss")}");
Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();

async Task Main()
{
    await Hire_Hop_Interface.Interface.Authentication.Login(cookie, HH_IC_ID_Sync.Auth.PMY.hh_email, HH_IC_ID_Sync.Auth.PMY.hh_pword);
    Console.WriteLine("HH Login Complete");

    await HH_IC_ID_Sync.IDSync.SyncJobIds(cookie);
}

Main().Wait();

DateTime end = DateTime.Now;
TimeSpan dur = end - start;
Console.WriteLine($"Sync Finished At {end.ToString("dd/MM/yyyy HH:mm:ss")} Taking {dur.TotalSeconds} Seconds");