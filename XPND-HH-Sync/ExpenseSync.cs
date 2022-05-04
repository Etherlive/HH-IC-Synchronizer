using Hire_Hop_Interface.Interface.Connections;
using Hire_Hop_Interface.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPND_HH_Sync
{
    public static class ExpenseSync
    {
        private const string ExpenseDesc = "Expend Expenses", ExpendSupplierID = "8335";
        public static int expensesSynced = 0;

        public static async Task Sync(Expense[] expenses, CookieConnection cookie)
        {
            var syncableExpenses = expenses.Where(x => x.JobIsNumerical).ToArray();

            var hhPOs = await Hire_Hop_Interface.Objects.PurchaseOrder.SearchForAll(cookie);
            var existingExpendHHPOs = hhPOs.results.Where(x => x.desc == ExpenseDesc).ToArray();

            var expensesHHJobIds = syncableExpenses.Select(x => x.Job).Distinct().ToArray();

            foreach (var jobId in expensesHHJobIds)
            {
                var expensesForJob = syncableExpenses.Where(x => x.Job == jobId).ToArray();

                var hhPO = existingExpendHHPOs.FirstOrDefault(x => x.JobId.ToString() == jobId);
                if (hhPO == null)
                {
                    hhPO = await PurchaseOrder.CreateNew(cookie, jobId, ExpenseDesc, "", ExpendSupplierID, DateTime.Now, DateTime.Now);
                }

                var expensesToSync = expensesForJob.Where(x => x.ApprovedOrPaid && !hhPO.items.Any(y => y.MEMO.Contains(x.ExpenseID))).ToArray();

                foreach (var exp in expensesToSync)
                {
                    int nominalCode = 25;
                    string memo = "";
                    if (exp.IsTravel)
                    {
                        nominalCode = 27;
                        memo = $"{exp.Person} Travelled From {exp.Origin} To {exp.Destination} Travelling {exp.DistanceKM} KM / {exp.DistanceM} M. Spent At {exp.Merchant} On {exp.Date.ToShortDateString()} Using {exp.PaymentMechanism}.";
                    }
                    else
                    {
                        memo = $"{exp.Person} Bought {exp.ExpenseCategory} At {exp.Merchant} On {exp.Date.ToShortDateString()} Using {exp.PaymentMechanism}.";
                    }

                    memo += $"\r\n{exp.ExpenseNote}\r\n{exp.Attachments}\r\nSynced From Expend {exp.ExpenseID}";

                    await hhPO.AddLineItem(cookie, 1, exp.AmountExcTax, exp.AmountExcTax, exp.TaxRate, 8, $"{exp.Person} - {exp.Date.ToShortDateString()} - {exp.ExpenseCategory}", memo, nominalCode);
                    expensesSynced++;
                }
            }
        }
    }
}
