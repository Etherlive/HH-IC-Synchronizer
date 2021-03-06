using Hire_Hop_Interface.Interface.Connections;
using ICompleat.Objects;
using Hire_Hop_Interface.Objects;
using System.Text.Json;

namespace Synchronizer
{
    public static class POSync
    {
        #region Methods

        private static async Task DeleteAllPOsBySync(CookieConnection cookie)
        {
            var hhPOs = await Hire_Hop_Interface.Objects.PurchaseOrder.SearchForAll(cookie);
            var hhPOsCreatedByMe = hhPOs.results.Where(x => x.CREATE_USER == "IC PO Sync");
            Task.WaitAll(hhPOsCreatedByMe.Select(x => x.Delete(cookie)).ToArray());
            Console.WriteLine($"Deleted All {hhPOsCreatedByMe.Count()} POs Created By Sync");
        }

        private static async Task<Transaction[]> GetTransactions(Auth auth)
        {
            var txs = await ICompleat.Objects.Transaction.GetTransactionsUntillAllAsync(auth);

            for (int i = 0; i < txs.Length; i += 100)
            {
                var t_detail = txs.Skip(i).Take(100).Select(x => x.LoadDetail(auth)).ToArray();
                Task.WaitAll(t_detail);
                for (int ii = 0; i < 100; i++)
                {
                    if (!t_detail[ii].Result)
                    {
                        txs[i + ii] = null;
                    }
                }
                Console.WriteLine($"Tx Detail Loaded {(float)i / txs.Length * 100:0.0}%");
            }

            Console.WriteLine("Loaded Transaction Detail");

            return txs;
        }

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

        static bool StringsSimilar(string s1, string s2, int maxLengthDiff = 5)
        {
            if (s1 == null || s2 == null || s1.Length == 0 || s2.Length == 0) return false;
            s1 = s1.Replace("  ", " ").ToLower().Trim();
            s2 = s2.Replace("  ", " ").ToLower().Trim();
            return s1 == s2 || ((s1.StartsWith(s2) || s2.StartsWith(s1)) && Math.Abs(s1.Length - s2.Length) <= maxLengthDiff);
        }

        static async Task<IEnumerable<PurchaseOrder>> PushPOs(SearchCollection<Contact> contacts, Transaction[] icPOs, CookieConnection cookie)
        {
            var HHSync = icPOs.Select(x =>
            {
                var c = contacts.results.FirstOrDefault(y => StringsSimilar(y.Company, x.SupplierName) || StringsSimilar(y.Name, x.SupplierName));
                if (c != null)
                {
                    return PurchaseOrder.CreateNew(cookie, x.JobId, x.Title, x.PurchaseOrderReference, c.Id, x.CreatedDate, x.DeliveryDate);
                }
                return null;
            }).Where(x => x != null)/*.Take(60 - posCreated)*/.ToArray();

            Task.WaitAll(HHSync);
            var newPOs = HHSync.Select(x => x.Result);
            return newPOs;
        }

        static async Task UpdatePOs(IEnumerable<Transaction> icPOsForJob, PurchaseOrder order, CookieConnection cookie)
        {
            var matchedOrder = icPOsForJob.FirstOrDefault(x => x.PurchaseOrderReference == order.SUPPLIER_REF);

            if (matchedOrder != null)
            {
                var addLines = matchedOrder.lines.Where(x => !order.items.Any(y => StringsSimilar(y.DESCRIPTION, x.Description, 1000))).Select(x => order.AddLineItem(cookie, x.Quantity, x.UnitCost, x.Net, 20, 8, x.Description, "Imported From IC")).ToArray();
                Task.WaitAll(addLines);
                lineItemsCreated += addLines.Length;

                int status = 8;

                if (matchedOrder.IsApproved) status = 2;
                else if (matchedOrder.IsSaved) status = 0;
                else if (matchedOrder.IsPending) status = 1;

                try
                {
                    if (order.STATUS != status)
                    {
                        await order.UpdateStatus(cookie, status);
                        statusSynced++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Updating PO {order.desc} - {order.ID} Failed With Exception: {e.ToString()}");
                }
            }
        }

        static int statusSynced = 0, posCreated = 0, lineItemsCreated = 0;

        public static async Task SyncPOs(CookieConnection cookie, Auth pmy_auth, Auth ethl_auth)
        {
            //await DeleteAllPOsBySync(cookie);

            Console.WriteLine("Fetching Transactions, Suppliers And Retrieving Contacts...");

            var t_txs = GetTransactions(pmy_auth);

            var t_contacts = Hire_Hop_Interface.Objects.Contact.SearchForAll(cookie);
            var t_pmy_suppliers = ICompleat.Objects.Supplier.GetSuppliersUntillAllAsync(pmy_auth);
            var t_eth_suppliers = ICompleat.Objects.Supplier.GetSuppliersUntillAllAsync(ethl_auth);
            var t_hhPOs = Hire_Hop_Interface.Objects.PurchaseOrder.SearchForAll(cookie);

            Task.WaitAll(new Task[] { t_txs, t_contacts, t_pmy_suppliers, t_eth_suppliers, t_hhPOs });

            var txWithJobIds = t_txs.Result.Where(x => x != null && x.JobId != null).ToArray();

            var txWithUniquePORef = txWithJobIds.Where(x => x.PurchaseOrderReference!=null && !txWithJobIds.Any(y => y.Id != x.Id && y.PurchaseOrderReference == x.PurchaseOrderReference)).ToArray();

            var contacts = t_contacts.Result;
            var suppliers = t_eth_suppliers.Result.Concat(t_pmy_suppliers.Result).ToArray();
            var hhPOs = t_hhPOs.Result;

            Console.WriteLine($"Fetched {txWithJobIds.Length} Transactions, {suppliers.Length} Suppliers From IC\nAnd {contacts.results.Length} Contacts, {hhPOs.results.Length} PO\'s From HH");

            var POs = txWithUniquePORef.Where(x => x.IsOrder);
            var Invoicess = txWithUniquePORef.Where(x => x.IsInvoice);

            var POsWithoutSyncedSupplier = POs.Where(x => !contacts.results.Any(
                y => StringsSimilar(y.Company, x.SupplierName) ||
                    StringsSimilar(y.Name, x.SupplierName)
                ));

            var suppliersSet = POsWithoutSyncedSupplier.Select(x => suppliers.FirstOrDefault(y => StringsSimilar(y.Name, x.SupplierName)));

            //LogUnmatchableSupplierNames(POsWithoutSyncedSupplier, suppliersSet);

            var suppliersToSync = suppliersSet.Where(x => x != null).DistinctBy(x => x.Code).ToArray();

            var supSyncs = suppliersToSync.Select(x => Hire_Hop_Interface.Objects.Contact.CreateNew(cookie, x.Name, $"{x.AddressLine1}\n{x.AddressLine2}\n{x.StateOrCounty}\n{x.PostcodeOrZip}\n{x.Country}", x.Telephone, x.Email)).ToArray();
            Task.WaitAll(supSyncs);

            Console.WriteLine($"Synced {supSyncs.Length} Contacts To HH");

            var jobIdsOfPOs = txWithUniquePORef.Select(x => x.JobId).Distinct();

            foreach (string id in jobIdsOfPOs)
            {
                var hhPOsForJob = hhPOs.results.Where(x => x.JobId.ToString() == id);
                var icPOsForJob = POs.Where(x => x.JobId.ToString() == id);

                var icPOsNotInHH = icPOsForJob.Where(x => !hhPOsForJob.Any(y => y.SUPPLIER_REF == x.PurchaseOrderReference)).ToArray();

                var newPOs = await PushPOs(contacts, icPOsNotInHH, cookie);

                Console.WriteLine($"Created {newPOs.Count()} POs for Job {id}");
                posCreated += newPOs.Count();

                var POsToSyncStatus = hhPOsForJob.Concat(newPOs);

                var sy = POsToSyncStatus.Where(x => x != null).Select(x => UpdatePOs(icPOsForJob, x, cookie)).ToArray();
                Task.WaitAll(sy);
            }

            Console.WriteLine($"Pushed {posCreated} New POs\nUpdated {statusSynced} PO Status\'\nAdded {lineItemsCreated} Line Items To POs");
        }

        #endregion Methods
    }
}