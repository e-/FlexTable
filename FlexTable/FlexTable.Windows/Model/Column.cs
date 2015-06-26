﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Column : INotifyPropertyChanged
    {
        private String name;
        public String Name { get { return name; } set { name = value; } }

        private Int32 index;
        public Int32 Index { get { return index; } set { index = value; } }

        private Double width;
        public Double Width { get { return width; } set { width = value; } }

        private Double x;
        public Double X { get { return x; } set { x = value; OnPropertyChanged("X"); } }

        private Boolean enabled = true;
        public Boolean Enabled { get { return enabled; } set { enabled = value; OnPropertyChanged("Enabled"); } }

        private Boolean highlighted = false;
        public Boolean Highlighted { get { return highlighted; } set { highlighted = value; OnPropertyChanged("Highlighted"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public ColumnType Type { get; set; }
        public String TypeString
        {
            get
            {
                return Type == ColumnType.Categorical ? "Categorical" : "Numerical";
            }
        }

        private List<Bin> bins;
        public List<Bin> Bins { get { return bins; } set { bins = value; } }

        private Double minValue;
        public Double MinValue { get { return minValue; } set { minValue = value; OnPropertyChanged("MinValue"); } }

        private Double maxValue;
        public Double MaxValue { get { return maxValue; } set { maxValue = value; OnPropertyChanged("MaxValue"); } }

        private Double meanValue;
        public Double MeanValue { get { return meanValue; } set { meanValue = value; OnPropertyChanged("MeanValue"); } }

        private Double medianValue;
        public Double MedianValue { get { return medianValue; } set { medianValue = value; OnPropertyChanged("MedianValue"); } }


        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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

        public static List<Bin> GetFrequencyBins(IEnumerable<String> cellValues)
        {
            Dictionary<String, Int32> dictionary = new Dictionary<String, Int32>();

            foreach (String value in cellValues)
            {
                if (!dictionary.ContainsKey(value))
                {
                    dictionary[value] = 0;
                }
                dictionary[value]++;
            }

            var sorted = dictionary.Keys.ToList();
            sorted.Sort();

            List<Bin> bins = new List<Bin>();
            foreach (String key in sorted)
            {
                bins.Add(new Bin() { Name = key, Count = dictionary[key] });
            }

            return bins;
        }

        public static List<Bin> GetHistogramBins(IEnumerable<Double> cellValues)
        {
            Double min = cellValues.Min(),
                   max = cellValues.Max();

            if (min == max) max = min + 1;

            Int32 binCount = 8;
            Double interval = (max - min) / binCount;
            
            List<Bin> bins = new List<Bin>();
            for (Int32 i = 0; i < binCount; ++i)
            {
                Double start = Math.Round(min + interval * i),
                       end = Math.Round(min + interval * (i+1))
                       ;

                bins.Add(new Bin(){
                    Name = String.Format("{0}-{1}", start, end),
                    Count = 0
                });
            }

            foreach (Double value in cellValues)
            {
                Int32 index = (Int32)Math.Floor((value - min) * binCount / (max - min));
                if (index >= binCount) index = binCount - 1;
                bins[index].Count++;
            }

            return bins;
        }
    }
}
