using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPND_HH_Sync
{
    public static class CSVParser
    {
        static string[] GetCells(string line)
        {
            List<string> cells = new List<string>();

            string temp = "";
            bool insideString = false;
            foreach (char c in line)
            {
                switch (c)
                {
                    case ',':
                        cells.Add(temp);
                        temp = "";
                        break;

                    case '\"':
                        insideString = !insideString;
                        break;

                    default:
                        temp += c;
                        break;
                }
            }

            if (temp.Length > 0) cells.Add(temp);

            return cells.ToArray();
        }
        public static Expense[] ParseFile(string fileContent)
        {
            List<Expense> expenses = new List<Expense>();
            string[] lines = fileContent.Split("\r\n");

            string[] headings = GetCells(lines[0]);

            foreach (string line in lines.Skip(1))
            {
                string[] cells = GetCells(line);

                if (cells.Length == headings.Length)
                {
                    expenses.Add(new Expense(cells));
                }
            }

            return expenses.ToArray();
        }
    }
}
