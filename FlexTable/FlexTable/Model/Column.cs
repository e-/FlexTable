using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Column : NotifyModel
    {
        private String name;
        public String Name { get { return name; } set { name = value; } }
       
        public static ColumnType GuessColumnType(IEnumerable<String> cellValues)
        {
            Boolean allDouble = true;
            Double result;
            List<String> differentValues = new List<String>();

            foreach (String value in cellValues)
            {
                if (differentValues.IndexOf(value) < 0)
                {
                    differentValues.Add(value);
                }

                if (!Double.TryParse(value, out result))
                {
                    allDouble = false;
                    break;
                }
            }

            // 문자가 하나라도 있으면 무조건 범주형
            if (!allDouble) return ColumnType.Categorical;

            if (differentValues.Count < 10) return ColumnType.Categorical;
            return ColumnType.Numerical;
        }

        public static Boolean CheckStringValue(IEnumerable<String> cellValues)
        {
            Boolean allDouble = true;
            Double result;

            foreach (String value in cellValues)
            {
                if (!Double.TryParse(value, out result))
                {
                    allDouble = false;
                    break;
                }
            }

            return !allDouble;
        }
    }
}
