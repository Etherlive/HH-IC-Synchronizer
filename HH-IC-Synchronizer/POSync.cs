using Hire_Hop_Interface.Interface.Connections;
using ICompleat.Objects;
using Hire_Hop_Interface.Objects;
using System.Text.Json;

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
            var txs = await ICompleat.Objects.Transaction.GetTransactionsUntillAllAsync();
            var contacts = await Hire_Hop_Interface.Objects.Contact.SearchForAll(cookie);
            var suppliers = await ICompleat.Objects.Supplier.GetSuppliersUntillAllAsync();

            Console.WriteLine($"Fetched {txs.Length} Transactions, {suppliers.Length} Suppliers From IC\nAnd {contacts.results.Length} Contacts From HH");

            for (int i = 0; i < txs.Length; i++)
            {
                try
                {
                    await txs[i].LoadDetail();
                }
                catch
                {
                    Console.WriteLine($"Error Loading {txs[i].Id} - {txs[i].Title}");
                }
                if (i % 50 == 0)
                {
                    Console.WriteLine($"Tx Detail Loaded {(float)i / txs.Length * 100:0.0}%");
                }
            }

            Console.WriteLine("Loaded Transaction Detail");

            var txWithJobIds = txs.Where(x => x.JobId != null);

            var POs = txWithJobIds.Where(x => x.IsOrder);
            var Invoicess = txWithJobIds.Where(x => x.IsInvoice);

            var POsWithoutSyncedSupplier = POs.Where(x => !contacts.results.Any(
                y => SupplierNameMatch(y.Company, x.SupplierName) ||
                    SupplierNameMatch(y.Name, x.SupplierName)
                ));

            var suppliersSet = POsWithoutSyncedSupplier.Select(x => suppliers.FirstOrDefault(y => SupplierNameMatch(y.Name, x.SupplierName)));

            //LogUnmatchableSupplierNames(POsWithoutSyncedSupplier, suppliersSet);

            var suppliersToSync = suppliersSet.Where(x => x != null).DistinctBy(x => x.Code).ToArray();

            var supSyncs = suppliersToSync.Select(x => Hire_Hop_Interface.Objects.Contact.CreateNew(cookie, x.Name, $"{x.AddressLine1}\n{x.AddressLine2}\n{x.StateOrCounty}\n{x.PostcodeOrZip}\n{x.Country}", x.Telephone, x.Email)).ToArray();
            Task.WaitAll(supSyncs);

            Console.WriteLine($"Synced {supSyncs.Length} Contacts To HH");

            var hhPOs = await Hire_Hop_Interface.Objects.PurchaseOrder.SearchForAll(cookie);

            var jobIdsOfPOs = txWithJobIds.Select(x => x.JobId).Distinct();

            List<PurchaseOrder> newPOs = new List<PurchaseOrder>();

            foreach (string id in jobIdsOfPOs)
            {
                var hhPOsForJob = hhPOs.results.Where(x => x.JobId.ToString() == id);
                var icPOsForJob = POs.Where(x => x.JobId.ToString() == id);

                var icPOsNotInHH = icPOsForJob.Where(x => hhPOsForJob.Any(y => y.SUPPLIER_REF == x.PurchaseOrderReference));

                var HHSync = icPOsNotInHH.Select(x => PurchaseOrder.CreateNew(cookie, x.JobId, x.Title, x.PurchaseOrderReference, x.CreatedDate, x.DeliveryDate)).ToArray();
                Task.WaitAll(HHSync);
                newPOs.AddRange(HHSync.Select(x => x.Result));
            }

            Console.WriteLine($"Pushed {newPOs.Count} POs To HH");
        }

        #endregion Methods
    }
}