﻿using FlexTable.Model;
using FlexTable.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace FlexTable.ViewModel
{
    public class FilterListViewModel : Notifiable
    {
        private String name;
        public String Name { get { return name; } set { name = value; OnPropertyChanged(nameof(Name)); } }

        public Func<Row, Boolean> Predicate { get; set; }
        private MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;

        public FilterListViewModel(MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public IEnumerable<Row> Filter(IEnumerable<Row> rows)
        {
            if (Predicate == null)
                throw new Exception("Predicate was not provided");

            return rows.Where(d => Predicate(d));
        }

        public static IEnumerable<Row> ApplyFilters(IEnumerable<FilterListViewModel> filters, IEnumerable<Row> rows)
        {
            foreach(FilterListViewModel filter in filters)
            {
                rows = filter.Filter(rows);
            }
            return rows;
        }
    }
}