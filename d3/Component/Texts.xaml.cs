using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace d3.Component
{
    public sealed partial class Texts : UserControl
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Data), typeof(Texts), new PropertyMetadata(null, new PropertyChangedCallback(DataChanged)));

        public Data Data
        {
            get { return (Data)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty WidthGetterProperty =
            DependencyProperty.Register("WidthGetter", typeof(Func<Object, Int32, Double>), typeof(Texts), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> WidthGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(WidthGetterProperty); }
            set { SetValue(WidthGetterProperty, value); }
        }

        public static readonly DependencyProperty HeightGetterProperty =
            DependencyProperty.Register("HeightGetter", typeof(Func<Object, Int32, Double>), typeof(Texts), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> HeightGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(HeightGetterProperty); }
            set { SetValue(HeightGetterProperty, value); }
        }

        public static readonly DependencyProperty XGetterProperty =
            DependencyProperty.Register("XGetter", typeof(Func<Object, Int32, Double>), typeof(Texts), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> XGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(XGetterProperty); }
            set { SetValue(XGetterProperty, value); }
        }

        public static readonly DependencyProperty YGetterProperty =
            DependencyProperty.Register("YGetter", typeof(Func<Object, Int32, Double>), typeof(Texts), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> YGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(YGetterProperty); }
            set { SetValue(YGetterProperty, value); }
        }

        public static readonly DependencyProperty ColorGetterProperty =
            DependencyProperty.Register("ColorGetter", typeof(Func<Object, Int32, Color>), typeof(Texts), new PropertyMetadata(default(Func<Object, Int32, Color>)));

        public Func<Object, Int32, Color> ColorGetter
        {
            get { return (Func<Object, Int32, Color>)GetValue(ColorGetterProperty); }
            set { SetValue(ColorGetterProperty, value); }
        }

        public static readonly DependencyProperty TextGetterProperty =
            DependencyProperty.Register("TextGetter", typeof(Func<Object, Int32, string>), typeof(Texts), new PropertyMetadata(default(Func<Object, Int32, String>)));

        public Func<Object, Int32, String> TextGetter
        {
            get { return (Func<Object, Int32, String>)GetValue(TextGetterProperty); }
            set { SetValue(TextGetterProperty, value); }
        }
        
        private static void DataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Texts textBlocks = source as Texts;
            textBlocks.Update();
        }

        /*public Double MaxActualWidth { get { 
            return borders.Select(tb => tb.ActualWidth).Max(); 
        } }*/

        List<Border> previousBorders = new List<Border>();
        List<TextBlock> textBlocks = new List<TextBlock>();
        List<Border> borders = new List<Border>();

        public Texts()
        {
            this.InitializeComponent();
        }

        public void Update()
        {
            foreach (Border border in previousBorders)
            {
                TextsCanvas.Children.Remove(border);
            }
            previousBorders.Clear();
            textBlocks.Clear();

            Int32 index = 0;
            foreach (Object datum in Data.List)
            {
                Border border = new Border();

                if (WidthGetter != null) border.Width = WidthGetter(datum, index);
                if (HeightGetter != null) border.Height = HeightGetter(datum, index);

                TextBlock textBlock = new TextBlock()
                {
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                    Text = TextGetter(datum, index),
                    Foreground = ColorGetter == null ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(ColorGetter(datum, index))
                };

                border.Child = textBlock;

                Canvas.SetLeft(border, XGetter(datum, index));
                Canvas.SetTop(border, YGetter(datum, index));
                index++;

                TextsCanvas.Children.Add(border);
                textBlocks.Add(textBlock);
                border.Measure(new Size(Double.MaxValue, Double.MaxValue));
                borders.Add(border);

                previousBorders.Add(border);
            }
        }
    }
}
