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

bool SupplierNameMatch(string s1, string s2)
{
    if (s1 == null || s2 == null || s1.Length == 0 || s2.Length == 0) return false;
    s1 = s1.Replace("  ", " ");
    s2 = s2.Replace("  ", " ");
    return s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase) || ((s1.StartsWith(s2) || s2.StartsWith(s1)) && Math.Abs(s1.Length - s2.Length) <= 5);
}

void LogUnmatchableSupplierNames(IEnumerable<Transaction> POsWithoutSyncedSupplier, IEnumerable<Supplier> suppliersSet)
{
    int i = 0;
    List<Transaction> unmatchableTx = new List<Transaction>();
    foreach (var tx in suppliersSet)
    {
        if (tx == null)
        {
            unmatchableTx.Add(POsWithoutSyncedSupplier.ElementAt(i));
        }
        i++;
    }
    var unmatchedSupplierNames = unmatchableTx.Select(x => x.SupplierName);
}

async Task SyncPOs()
{
    var r = await ICompleat.Objects.Transaction.GetTransactionsUntillAllAsync();
    var c = await Hire_Hop_Interface.Objects.Contact.SearchForAll(cookie);
    var s = await ICompleat.Objects.Supplier.GetSuppliersUntillAllAsync();

    Console.WriteLine($"Fetched {r.Length} Transactions From IC");

    var POs = r.Where(x => x.IsOrder);
    var Invoicess = r.Where(x => x.IsInvoice);

    var POsWithoutSyncedSupplier = POs.Where(x => !c.results.Any(
        y => SupplierNameMatch(y.Company, x.SupplierName) ||
            SupplierNameMatch(y.Name, x.SupplierName)
        ));

    try
    {
        var suppliersSet = POsWithoutSyncedSupplier.Select(x => s.FirstOrDefault(y => SupplierNameMatch(y.Name, x.SupplierName)));

        //LogUnmatchableSupplierNames(POsWithoutSyncedSupplier, suppliersSet);

        var suppliersToSync = suppliersSet.Where(x => x != null).DistinctBy(x => x.Code).ToArray();

        var supSyncs = suppliersToSync.Select(x => Hire_Hop_Interface.Objects.Contact.CreateNew(cookie, x.Name, $"{x.AddressLine1}\n{x.AddressLine2}\n{x.StateOrCounty}\n{x.PostcodeOrZip}\n{x.Country}", x.Telephone, x.Email)).ToArray();
        Task.WaitAll(supSyncs);

        Console.WriteLine($"Synced {supSyncs.Length} Contacts To HH");
    }
    catch (Exception e)
    {
        e = e;
    }
}

var t = new Task[] { SyncJobIds(), SyncPOs() };

Task.WaitAll(t);

DateTime end = DateTime.Now;
TimeSpan dur = end - start;
Console.WriteLine($"Sync Finished At {end.ToString("dd/MM/yyyy HH:mm:ss")} Taking {dur.TotalSeconds} Seconds");