﻿using Hire_Hop_Interface.Interface.Connections;
using ICompleat.Objects;
using Hire_Hop_Interface.Objects;
using System.Text.Json;

namespace IC_HH_PO_Sync
{
    public static class POSync
    {
        #region Methods

        static async Task LoadTxDetail_s(Transaction t)
        {
            try
            {
                await t.LoadDetail();
            }
            catch
            {
                Console.WriteLine($"Error Loading {t.Id} - {t.Title}");
            }
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
            s1 = s1.Replace("  ", " ");
            s2 = s2.Replace("  ", " ");
            return s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase) || ((s1.StartsWith(s2) || s2.StartsWith(s1)) && Math.Abs(s1.Length - s2.Length) <= maxLengthDiff);
        }

        public static async Task SyncPOs(CookieConnection cookie)
        {
            Console.WriteLine("Fetching Transactions, Suppliers And Retrieving Contacts...");

            var t_txs = ICompleat.Objects.Transaction.GetTransactionsUntillAllAsync();
            var t_contacts = Hire_Hop_Interface.Objects.Contact.SearchForAll(cookie);
            var t_suppliers = ICompleat.Objects.Supplier.GetSuppliersUntillAllAsync();
            var t_hhPOs = Hire_Hop_Interface.Objects.PurchaseOrder.SearchForAll(cookie);

            Task.WaitAll(new Task[] { t_txs, t_contacts, t_suppliers, t_hhPOs });

            var txs = t_txs.Result;
            var contacts = t_contacts.Result;
            var suppliers = t_suppliers.Result;
            var hhPOs = t_hhPOs.Result;

            Console.WriteLine($"Fetched {txs.Length} Transactions, {suppliers.Length} Suppliers From IC\nAnd {contacts.results.Length} Contacts, {hhPOs.results.Length} PO\'s From HH");

            for (int i = 0; i < txs.Length; i += 100)
            {
                var t_detail = txs.Skip(i).Take(100).Select(x => LoadTxDetail_s(x)).ToArray();
                Task.WaitAll(t_detail);
                Console.WriteLine($"Tx Detail Loaded {(float)i / txs.Length * 100:0.0}%");
            }

            Console.WriteLine("Loaded Transaction Detail");

            var txWithJobIds = txs.Where(x => x.JobId != null);

            var POs = txWithJobIds.Where(x => x.IsOrder);
            var Invoicess = txWithJobIds.Where(x => x.IsInvoice);

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

            var jobIdsOfPOs = txWithJobIds.Select(x => x.JobId).Distinct();

            int statusSynced = 0, posCreated = 0;

            foreach (string id in jobIdsOfPOs)
            {
                var hhPOsForJob = hhPOs.results.Where(x => x.JobId.ToString() == id);
                var icPOsForJob = POs.Where(x => x.JobId.ToString() == id);

                var icPOsNotInHH = icPOsForJob.Where(x => !hhPOsForJob.Any(y => y.SUPPLIER_REF == x.IdentifierReference));

                var HHSync = icPOsNotInHH.Select(x =>
                {
                    var c = contacts.results.FirstOrDefault(y => StringsSimilar(y.Company, x.SupplierName) || StringsSimilar(y.Name, x.SupplierName));
                    if (c != null)
                    {
                        return PurchaseOrder.CreateNew(cookie, x.JobId, x.Title, x.IdentifierReference, c.Id, x.CreatedDate, x.DeliveryDate);
                    }
                    return null;
                }).Where(x => x != null).Take(60 - posCreated).ToArray();

                Task.WaitAll(HHSync);
                var newPOs = HHSync.Select(x => x.Result);

                posCreated += newPOs.Count();

                var POsToSyncStatus = hhPOsForJob.Concat(newPOs);

                foreach (PurchaseOrder order in POsToSyncStatus.Where(x => x != null))
                {
                    var matchedOrder = icPOsForJob.FirstOrDefault(x => x.IdentifierReference == order.SUPPLIER_REF);

                    if (matchedOrder != null)
                    {
                        var addLines = matchedOrder.lines.Where(x => !order.items.Any(y => StringsSimilar(y.DESCRIPTION, x.Description, 100))).Select(x => order.AddLineItem(cookie, x.Quantity, x.UnitCost, x.Net, 20, 8, x.Description)).ToArray();
                        Task.WaitAll(addLines);

                        int status = 8;

                        if (matchedOrder.IsApproved) status = 2;
                        else if (matchedOrder.IsSaved) status = 0;
                        else if (matchedOrder.IsPending) status = 1;

                        try
                        {
                            await order.UpdateStatus(cookie, status);
                            statusSynced++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Updating PO {order.desc} - {order.ID} Failed With Exception: {e.ToString()}");
                        }
                    }
                }
            }

            Console.WriteLine($"Pushed {posCreated} New POs and Updated {statusSynced} PO Status\'");
        }

        #endregion Methods
    }
}