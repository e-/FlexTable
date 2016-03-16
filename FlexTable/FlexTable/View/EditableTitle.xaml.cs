using FlexTable.Util;
using FlexTable.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.View
{
    public sealed partial class EditableTitle : UserControl
    {
        Drawable drawable = new Drawable();
        PageViewModel ViewModel => (PageViewModel)DataContext;

        public EditableTitle()
        {
            this.InitializeComponent();

            drawable.Attach(Root, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += Drawable_StrokeAdded;
        }

        private void Drawable_StrokeAdded(InkManager inkManager)
        {
            if (inkManager.GetStrokes().Count > 0) {
                var stroke = inkManager.GetStrokes()[0];
                Double centerX = stroke.BoundingRect.Left + stroke.BoundingRect.Width / 2;

                if (stroke.BoundingRect.Height < 30)
                {
                    foreach (UIElement element in TitleContainer.Children)
                    {
                        if (element is ComboBox)
                        {
                            ComboBox comboBox = (ComboBox)element;
                            element.Measure(new Size(1000, 1000));
                            Point position = comboBox.TransformToVisual(Root).TransformPoint(new Point(0, 0));

                            if (position.X <= centerX && centerX < position.X + comboBox.ActualWidth)
                            {
                                if (comboBox.Tag != null && ((String)comboBox.Tag).Length > 0)
                                {
                                    IEnumerable<ColumnViewModel> candidates = ViewModel.MainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Column.Name == (String)comboBox.Tag);

                                    if(candidates.Count() > 0)
                                    {
                                        ColumnViewModel cvm = candidates.First();
                                        ViewModel.RemoveColumnViewModel(cvm);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            drawable.RemoveAllStrokes();
        }

        public void Clear()
        {
            TitleContainer.Children.Clear();
        }

        public void Add(UIElement ele)
        {
            TitleContainer.Children.Add(ele);
        }
    }
}
