DateTime start = DateTime.Now;
Console.WriteLine($"Sync Started At {start.ToString("dd/MM/yyyy HH:mm:ss")}");

Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();

async Task Main()
{
    await Hire_Hop_Interface.Interface.Authentication.Login(cookie, Synchronizer.Auth.PMY.hh_email, Synchronizer.Auth.PMY.hh_pword);
    Console.WriteLine("HH Login Complete");

    if (!await Hire_Hop_Interface.Interface.Authentication.ToggleAdmin(cookie))
    {
        await Hire_Hop_Interface.Interface.Authentication.ToggleAdmin(cookie);
    }

    Console.WriteLine("Syncing HH IDs To IC");
    await Synchronizer.IDSync.SyncJobIds(cookie);

    //Sync POs only every 1st 15 Minute Run
    if (start.Minute / 15.0f < 1)
    {
        Console.WriteLine("Syncing IC POs To HH");
        await Synchronizer.POSync.SyncPOs(cookie, Synchronizer.Auth.PMY, Synchronizer.Auth.ETHL);
    }
}

Main().Wait();

DateTime end = DateTime.Now;
TimeSpan dur = end - start;
Console.WriteLine($"Sync Finished At {end.ToString("dd/MM/yyyy HH:mm:ss")} Taking {dur.TotalSeconds} Seconds");