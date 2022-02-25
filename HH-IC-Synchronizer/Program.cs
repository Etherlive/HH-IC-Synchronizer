// See https://aka.ms/new-console-template for more information
using Hire_Hop_Interface.Interface;
using Hire_Hop_Interface.Objects.JobProject;
using ICompleat;
using ICompleat.Objects;
using System.Linq;

async Task Main()
{
    DateTime start = DateTime.Now; ;
    Console.WriteLine($"Sync Started At {start.ToString("dd/MM/yyyy HH:mm:ss")}");

    Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();
    await Hire_Hop_Interface.Interface.Authentication.Login(cookie, HH_IC_Synchronizer.Config.hh_email, HH_IC_Synchronizer.Config.hh_pword);

    Console.WriteLine("HH Login Complete");

    var r = await SearchResult.SearchForAll(new SearchResult.SearchOptions(), cookie);
    var vals = r.results.OrderByDescending(x => x.jobDate).Select(x => new ICompleat.Objects.CustomFields.Field() { Code = x.id.Substring(1), Name = x.job_name.ToLower().Replace("&lt;", "").Replace("&gt;", "").Replace("&amp;", "") }).ToList();

    for (int i = 0; i < vals.Count; i++)
    {
        int matches = vals.Take(i).Count(x => x.Name.ToLower().Split("%-")[0] == vals[i].Name.ToLower());
        vals[i].Name = matches > 0 ? vals[i].Name + "%-" + matches : vals[i].Name;
    }

    Console.WriteLine($"Fetched {vals.Count} Jobs From HH");

    var fields = await ICompleat.Objects.CustomFields.GetCustomFieldsAsync();
    await fields[0].ReplaceValues(vals);

    DateTime end = DateTime.Now;
    TimeSpan dur = end - start;
    Console.WriteLine($"Sync Finished At {end.ToString("dd/MM/yyyy HH:mm:ss")} Taking {dur.TotalSeconds} Seconds");
}

var t = Main();

t.Wait();