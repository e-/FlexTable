﻿using FlexTable.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class MetadataViewModel : Notifiable
    {
        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

        private String sheetName;
        public String SheetName { get { return sheetName; } set { sheetName = value; OnPropertyChanged(nameof(SheetName)); } }

        private Int32 columnCount;
        public Int32 ColumnCount { get { return columnCount; } set { columnCount = value; OnPropertyChanged(nameof(ColumnCount)); } }

        private Int32 categoricalColumnCount;
        public Int32 CategoricalColumnCount { get { return categoricalColumnCount; } set { categoricalColumnCount = value; OnPropertyChanged(nameof(CategoricalColumnCount)); } }

        private Int32 numericalColumnCount;
        public Int32 NumericalColumnCount { get { return numericalColumnCount; } set { numericalColumnCount = value; OnPropertyChanged(nameof(NumericalColumnCount)); } }

        private Int32 rowCount;
        public Int32 RowCount { get { return rowCount; } set { rowCount = value; OnPropertyChanged(nameof(RowCount)); } }

        public MetadataViewModel(MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void Initialize()
        {
            Model.Sheet sheet = mainPageViewModel.Sheet;
            SheetName = sheet.Name;
            ColumnCount = sheet.Columns.Count;
            RowCount = sheet.Rows.Count;
            CategoricalColumnCount = mainPageViewModel.SheetViewModel.ColumnViewModels.Count(c => c.Type == Model.ColumnType.Categorical);
            NumericalColumnCount = mainPageViewModel.SheetViewModel.ColumnViewModels.Count(c => c.Type == Model.ColumnType.Numerical);
        }
    }
}
