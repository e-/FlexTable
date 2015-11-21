using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using FlexTable.Model;
using FlexTable.ViewModel;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class MetedataView : UserControl
    {
        public MetedataView()
        {
            this.InitializeComponent();
        }

        private async void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await Util.FileLoader.Instance.Open();
            if (file == null)
                return;
            Sheet sheet = await Util.CsvLoader.Load(file);

            ViewModel.MainPageViewModel mpvm = (this.DataContext as ViewModel.MetadataViewModel).MainPageViewModel;

            mpvm.View.TableView.TopColumnHeader.Reset();
            mpvm.View.TableView.BottomColumnHeader.Reset();
            mpvm.View.TableView.ColumnIndexer.Reset();
            mpvm.ExplorationViewModel.TopPageView.PageViewModel.State = PageViewModel.PageViewState.Empty;            
            //mpvm.ExplorationViewModel.TopPageView.CancelUndoStoryboard.Begin();
            mpvm.Sheet = sheet;
            mpvm.Initialize();
        }
    }
}
