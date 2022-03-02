// See https://aka.ms/new-console-template for more information

DateTime start = DateTime.Now;
Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();

async Task Start()
{
    Console.WriteLine($"Sync Started At {start.ToString("dd/MM/yyyy HH:mm:ss")}");

    await Hire_Hop_Interface.Interface.Authentication.Login(cookie, HH_IC_Synchronizer.Config.hh_email, HH_IC_Synchronizer.Config.hh_pword);
    Console.WriteLine("HH Login Complete");
}

Start().Wait();

var t = new Task[] { HH_IC_Synchronizer.IDSync.SyncJobIds(cookie), HH_IC_Synchronizer.POSync.SyncPOs(cookie) };

Task.WaitAll(t);

if (t.Any(x => x.IsFaulted))
{
    foreach (Exception e in t.Where(x => x.IsFaulted).Select(x => x.Exception))
    {
        Console.WriteLine("An Error Occurred\n" + e.ToString());
    }
}

DateTime end = DateTime.Now;
TimeSpan dur = end - start;
Console.WriteLine($"Sync Finished At {end.ToString("dd/MM/yyyy HH:mm:ss")} Taking {dur.TotalSeconds} Seconds");