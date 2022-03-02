using Hire_Hop_Interface.Interface.Connections;
using ICompleat.Objects;

namespace HH_IC_Synchronizer
{
    public static class POSync
    {
        #region Methods

        static void LogUnmatchableSupplierNames(IEnumerable<ICompleat.Objects.Transaction> POsWithoutSyncedSupplier, IEnumerable<Supplier> suppliersSet)
        {
            int i = 0;
            List<ICompleat.Objects.Transaction> unmatchableTx = new List<ICompleat.Objects.Transaction>();
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

        static bool SupplierNameMatch(string s1, string s2)
        {
            if (s1 == null || s2 == null || s1.Length == 0 || s2.Length == 0) return false;
            s1 = s1.Replace("  ", " ");
            s2 = s2.Replace("  ", " ");
            return s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase) || ((s1.StartsWith(s2) || s2.StartsWith(s1)) && Math.Abs(s1.Length - s2.Length) <= 5);
        }

        public static async Task SyncPOs(CookieConnection cookie)
        {
            Console.WriteLine("Fetching Transactions, Suppliers And Retrieving Contacts...");
            var r = await ICompleat.Objects.Transaction.GetTransactionsUntillAllAsync();
            var c = await Hire_Hop_Interface.Objects.Contact.SearchForAll(cookie);
            var s = await ICompleat.Objects.Supplier.GetSuppliersUntillAllAsync();

            Console.WriteLine($"Fetched {r.Length} Transactions, {s.Length} Suppliers From IC\nAnd {c.results.Length} Contacts From HH");

            var POs = r.Where(x => x.IsOrder);
            var Invoicess = r.Where(x => x.IsInvoice);

            var POsWithoutSyncedSupplier = POs.Where(x => !c.results.Any(
                y => SupplierNameMatch(y.Company, x.SupplierName) ||
                    SupplierNameMatch(y.Name, x.SupplierName)
                ));

            var suppliersSet = POsWithoutSyncedSupplier.Select(x => s.FirstOrDefault(y => SupplierNameMatch(y.Name, x.SupplierName)));

            //LogUnmatchableSupplierNames(POsWithoutSyncedSupplier, suppliersSet);

            var suppliersToSync = suppliersSet.Where(x => x != null).DistinctBy(x => x.Code).ToArray();

            var supSyncs = suppliersToSync.Select(x => Hire_Hop_Interface.Objects.Contact.CreateNew(cookie, x.Name, $"{x.AddressLine1}\n{x.AddressLine2}\n{x.StateOrCounty}\n{x.PostcodeOrZip}\n{x.Country}", x.Telephone, x.Email)).ToArray();
            Task.WaitAll(supSyncs);

            Console.WriteLine($"Synced {supSyncs.Length} Contacts To HH");
        }

        #endregion Methods
    }
}