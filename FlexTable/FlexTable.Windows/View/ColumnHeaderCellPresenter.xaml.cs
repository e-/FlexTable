﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class ColumnHeaderCellPresenter : UserControl
    {
        Dictionary<uint, Pointer> contacts = new Dictionary<uint, Pointer>();
        uint numActiveContacts=0;

        public ColumnHeaderCellPresenter()
        {
            this.InitializeComponent();
        }

        public void Update()
        {
            XAnimation.Begin();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (contacts.ContainsKey(e.Pointer.PointerId)) { return; }
            contacts[e.Pointer.PointerId] = e.Pointer;
            ++numActiveContacts;
            (DataContext as Model.Column).Highlighted = true;
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (contacts.ContainsKey(e.Pointer.PointerId))
            {
                contacts[e.Pointer.PointerId] = null;
                contacts.Remove(e.Pointer.PointerId);
                --numActiveContacts;
            }
            
            if(numActiveContacts == 0) (DataContext as Model.Column).Highlighted = false;
        }

        private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (contacts.ContainsKey(e.Pointer.PointerId)) { return; }
            contacts[e.Pointer.PointerId] = e.Pointer;
            ++numActiveContacts;
            (DataContext as Model.Column).Highlighted = true;
        }

        private void Border_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (contacts.ContainsKey(e.Pointer.PointerId))
            {
                contacts[e.Pointer.PointerId] = null;
                contacts.Remove(e.Pointer.PointerId);
                --numActiveContacts;
            }

            if (numActiveContacts == 0) (DataContext as Model.Column).Highlighted = false;
        }

        private void Border_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (contacts.ContainsKey(e.Pointer.PointerId))
            {
                contacts[e.Pointer.PointerId] = null;
                contacts.Remove(e.Pointer.PointerId);
                --numActiveContacts;
            }

            if (numActiveContacts == 0) (DataContext as Model.Column).Highlighted = false;
        }
    }
}
