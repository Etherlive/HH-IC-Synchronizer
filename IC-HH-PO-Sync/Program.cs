﻿DateTime start = DateTime.Now;
Console.WriteLine($"Sync Started At {start.ToString("dd/MM/yyyy HH:mm:ss")}");

Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();

async Task Main()
{
    await Hire_Hop_Interface.Interface.Authentication.Login(cookie, IC_HH_PO_Sync.Auth.PMY.hh_email, IC_HH_PO_Sync.Auth.PMY.hh_pword);
    Console.WriteLine("HH Login Complete");

    if (!await Hire_Hop_Interface.Interface.Authentication.ToggleAdmin(cookie))
    {
        await Hire_Hop_Interface.Interface.Authentication.ToggleAdmin(cookie);
    }

    await IC_HH_PO_Sync.POSync.SyncPOs(cookie, IC_HH_PO_Sync.Auth.PMY, IC_HH_PO_Sync.Auth.ETHL);
}

Main().Wait();

DateTime end = DateTime.Now;
TimeSpan dur = end - start;
Console.WriteLine($"Sync Finished At {end.ToString("dd/MM/yyyy HH:mm:ss")} Taking {dur.TotalSeconds} Seconds");