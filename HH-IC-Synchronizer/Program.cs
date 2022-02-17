// See https://aka.ms/new-console-template for more information
using Hire_Hop_Interface.Interface;
using Hire_Hop_Interface.Objects.JobProject;
using ICompleat;
using ICompleat.Objects;
using System.Linq;

async void Main()
{
    Hire_Hop_Interface.Interface.Connections.CookieConnection cookie = new Hire_Hop_Interface.Interface.Connections.CookieConnection();
    await Hire_Hop_Interface.Interface.Authentication.Login(cookie, "odavies@etherlive.co.uk", "SomerSet876!%");
    var r = await SearchResult.SearchForAll(new SearchResult.SearchOptions(), cookie);
    var vals = r.results.Select(x => new ICompleat.Objects.CustomFields.Field() { Code = x.id.Substring(1), Name = x.job_name }).ToList();
    var fields = await ICompleat.Objects.CustomFields.GetCustomFieldsAsync();
    await fields[0].ReplaceValues(vals);
    Console.WriteLine("Done");
}

Main();

while (true)
{
    System.Threading.Thread.Sleep(1000);
}