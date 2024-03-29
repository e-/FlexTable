﻿using FlexTable.Model;
using FlexTable.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace FlexTable.ViewModel
{
    public class RowViewModel : Notifiable
    {
        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

        private Int32 index;

        // Index의 경우 초기 모든 row에는 row의 ID와 일치하나 뒤에 동적으로 생성된 rvm에 대해서는 해당하는 row가 없으므로 0부터 새로 붙인다.
        public Int32 Index
        {
            get { return index; }
            set
            {
                index = value;
                OnPropertyChanged(nameof(Index));
            }
        }
        public Double StashedY { get; set; }
        public Double Y { get { return index * Const.RowHeight; } }
        public Row Row { get; set; }
        public IEnumerable<Row> Rows { get; set; }

        private List<Cell> cells = new List<Cell>();
        public List<Cell> Cells { get { return cells; } }
        public Color StashedColor { get; set; }
        public Color Color { get; set; } = Colors.Black;

        public RowViewModel(MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void Stash()
        {
            StashedY = Y;
            StashedColor = Color;
        }
    }
}
