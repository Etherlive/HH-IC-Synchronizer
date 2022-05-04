using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPND_HH_Sync
{
    public class Expense
    {
        public string ExpenseID, Type, TransactionID, Person, Merchant, Origin, Destination, ClientLegacy, Client, Job, ExpenseCategory, ExpenseNote, Currency, LocalCurrency, TransactionCountry, Team, PaymentMechanism, ExpenseState, Attachments;
        public float DistanceKM, DistanceM, AmountExcTax, TaxRate, TaxAmount, AmountIncTax, LocalAmount, ExchangeRate;
        public bool ExpenseSubmitted, Rebilled;
        public DateTime Date;

        public Expense(string[] cells)
        {
            float r = -1;

            ExpenseID = cells[0];
            Type = cells[1];
            TransactionID = cells[2];
            Date = DateTime.Parse(cells[3]);
            Person = cells[4];
            Merchant = cells[5];
            Origin = cells[6];
            Destination = cells[7];
            DistanceKM = float.TryParse(cells[8], out r) ? r : -1;
            DistanceM = float.TryParse(cells[9], out r) ? r : -1;
            ClientLegacy = cells[10];
            Client = cells[11];
            Rebilled = cells[12] == "TRUE";
            Job = cells[13];
            ExpenseCategory = cells[14];
            ExpenseNote = cells[15];
            AmountExcTax = float.TryParse(cells[16], out r) ? r : -1;
            TaxRate = float.TryParse(cells[17], out r) ? r : -1;
            TaxAmount = float.TryParse(cells[18], out r) ? r : -1;
            AmountIncTax = float.TryParse(cells[19], out r) ? r : -1;
            Currency = cells[20];
            LocalAmount = float.TryParse(cells[21], out r) ? r : -1;
            LocalCurrency = cells[22];
            ExchangeRate = float.TryParse(cells[23], out r) ? r : -1;
            TransactionCountry = cells[24];
            Team = cells[25];
            PaymentMechanism = cells[26];
            ExpenseSubmitted = cells[27] == "TRUE";
            ExpenseState = cells[28];
            Attachments = cells[29];
        }

    }
}
