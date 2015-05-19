using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace FlexTable.Util
{
    public abstract class BooleanConverter<T> : IValueConverter
    {
        protected BooleanConverter(T trueValue, T falseValue)
        {
            this.TrueValue = trueValue;
            this.FalseValue = falseValue;
        }

        public T TrueValue { get; set; }

        public T FalseValue { get; set; }

        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            return Object.Equals(value, true) ?
                this.TrueValue : this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            return Object.Equals(value, this.TrueValue) ? true : false;
        }
    }
}
