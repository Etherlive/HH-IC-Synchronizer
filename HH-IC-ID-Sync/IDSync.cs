using Hire_Hop_Interface.Interface.Connections;
using Hire_Hop_Interface.Objects.JobProject;

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

            var fields = await ICompleat.Objects.CustomFields.GetCustomFieldsAsync();
            await fields[0].ReplaceValues(vals);

            ICompleat.Config._instance.companyId = Config.ic_elth_comp;
            ICompleat.Config._instance.tenantId = Config.ic_elth_tenn;
            ICompleat.Config._instance.key = Config.ic_elth_key;

            var fields_2 = await ICompleat.Objects.CustomFields.GetCustomFieldsAsync();
            await fields_2[0].ReplaceValues(vals);

            Console.WriteLine($"Pushed Custom Fields");
        }

        #endregion Methods
    }
}