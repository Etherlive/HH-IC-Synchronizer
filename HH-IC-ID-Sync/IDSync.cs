using Hire_Hop_Interface.Interface.Connections;
using Hire_Hop_Interface.Objects.JobProject;
using System.Diagnostics;

namespace HH_IC_ID_Sync
{
    public static class IDSync
    {
        #region Methods

        public static async Task SyncJobIds(CookieConnection cookie)
        {
            var r = await SearchResult.SearchForAll(new SearchResult.SearchOptions(), cookie);
            var vals = r.results.OrderByDescending(x => x.jobDate).Select(x => new ICompleat.Objects.CustomFields.Field() { Code = x.id.Substring(1), Name = x.id.Substring(1) + " - " + x.job_name.ToLower().Replace("&lt;", "").Replace("&gt;", "").Replace("&amp;", "") }).ToList();

            Console.WriteLine($"Fetched {vals.Count} Jobs From HH");

            var fields = await ICompleat.Objects.CustomFields.GetCustomFieldsAsync(Auth.PMY);
            await fields[0].ReplaceValues(vals, Auth.PMY);

            var fields_2 = await ICompleat.Objects.CustomFields.GetCustomFieldsAsync(Auth.ETHL);
            await fields_2[0].ReplaceValues(vals, Auth.ETHL);

            Console.WriteLine($"Pushed Custom Fields");

            if (Debugger.IsAttached)
            {
                string csv = String.Join('\n', r.results.Select(x => $"{x.id.Substring(1)},{x.job_name}"));
                File.WriteAllText("./job_ids.csv", csv);
                Console.WriteLine("Saved Job Ids");
            }
        }

        #endregion Methods
    }
}