using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace FlexTable.Util
{
    public abstract class ParityConverter<T> : IValueConverter
    {
        protected ParityConverter(T evenValue, T oddValue)
        {
            this.EvenValue = evenValue;
            this.OddValue = oddValue;
        }

        public T EvenValue { get; set; }

        public T OddValue { get; set; }

        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            return (Int32)value % 2 == 0 ?
                this.EvenValue : this.OddValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            return Object.Equals(value, this.EvenValue) ? 0: 1;
        }
    }
}
