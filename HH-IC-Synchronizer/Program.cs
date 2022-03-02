// See https://aka.ms/new-console-template for more information
using Hire_Hop_Interface.Interface;
using Hire_Hop_Interface.Objects.JobProject;
using ICompleat;
using ICompleat.Objects;
using System.Linq;

DateTime start = DateTime.Now;
Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();

async Task Start()
{
    Console.WriteLine($"Sync Started At {start.ToString("dd/MM/yyyy HH:mm:ss")}");

    await Hire_Hop_Interface.Interface.Authentication.Login(cookie, HH_IC_Synchronizer.Config.hh_email, HH_IC_Synchronizer.Config.hh_pword);
    Console.WriteLine("HH Login Complete");
}

Start().Wait();

async Task SyncJobIds()
{
    var r = await SearchResult.SearchForAll(new SearchResult.SearchOptions(), cookie);
    var vals = r.results.OrderByDescending(x => x.jobDate).Select(x => new ICompleat.Objects.CustomFields.Field() { Code = x.id.Substring(1), Name = x.id.Substring(1) + " - " + x.job_name.ToLower().Replace("&lt;", "").Replace("&gt;", "").Replace("&amp;", "") }).ToList();

    Console.WriteLine($"Fetched {vals.Count} Jobs From HH");

    var fields = await ICompleat.Objects.CustomFields.GetCustomFieldsAsync();
    await fields[0].ReplaceValues(vals);

    Console.WriteLine($"Pushed Custom Fields");
}

async Task SyncPOs()
{
    var r = await ICompleat.Objects.Transaction.GetTransactionsUntillAllAsync();
    var c = await Hire_Hop_Interface.Objects.Contact.SearchForAll(cookie);

    Console.WriteLine($"Fetched {r.Length} Transactions From IC");

    var POs = r.Where(x => x.IsOrder);
    var Invoicess = r.Where(x => x.IsInvoice);

    var POsWithoutSyncedSupplier = POs.Where(x => !c.results.Any(
        y => y.Company.Equals(x.SupplierName, StringComparison.InvariantCultureIgnoreCase) ||
            y.Name.Equals(x.SupplierName, StringComparison.InvariantCultureIgnoreCase)
        ));
}

var t = new Task[] { /*SyncJobIds(),*/ SyncPOs() };

Task.WaitAll(t);

DateTime end = DateTime.Now;
TimeSpan dur = end - start;
Console.WriteLine($"Sync Finished At {end.ToString("dd/MM/yyyy HH:mm:ss")} Taking {dur.TotalSeconds} Seconds");