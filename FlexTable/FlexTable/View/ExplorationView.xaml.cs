using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FlexTable.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class ExplorationView : UserControl
    {
        private PageView topPageView;
        public PageView TopPageView { get { return topPageView; } }
        public PageViewModel TopPageViewModel { get { return topPageView.ViewModel; } }
        public ExplorationViewModel ViewModel { get { return (ExplorationViewModel)DataContext; } }

        public ExplorationView()
        {
            this.InitializeComponent();
        }

        public void Initialize()
        {
            List<UIElement> victim = new List<UIElement>();

            foreach(UIElement ele in PageViewElement.Children)
            {
                if (ele is PageView) victim.Add(ele);
            }

            foreach (UIElement ele in victim) PageViewElement.Children.Remove(ele);

            AddNewPage();
        }

        public void AddNewPage()
        {
            PageView page = new PageView();
            MainPageViewModel mainPageViewModel = (this.DataContext as ExplorationViewModel).MainPageViewModel;
            PageViewModel pageViewModel = new PageViewModel(
                mainPageViewModel,
                page
                );
            topPageView = page;
            page.DataContext = pageViewModel;
            Canvas.SetTop(page, mainPageViewModel.PageOffset);
            Canvas.SetZIndex(page, 0);
            page.Initialize();

            PageViewElement.Children.Add(page);
        }

        public void RemoveTopPage(PageView nextTopPageView)
        {
            PageView currentTopPageView = topPageView;
            topPageView = nextTopPageView;

            PageViewElement.Children.Remove(currentTopPageView);
        }        

        public void RemoveNonTopPage(PageView pageView)
        {
            PageViewElement.Children.Remove(pageView);
        }
    }
}
