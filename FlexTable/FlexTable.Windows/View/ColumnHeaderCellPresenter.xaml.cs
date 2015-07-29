using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace FlexTable.View
{
    class ColumnHeaderHighlightConverter : Util.BooleanConverter<SolidColorBrush>
    {
        public ColumnHeaderHighlightConverter() : base(new SolidColorBrush(Colors.LightGray), new SolidColorBrush(Color.FromArgb(255, 236, 236, 236))) { }
    }

    public sealed partial class ColumnHeaderCellPresenter : UserControl
    {
        Dictionary<uint, Pointer> contacts = new Dictionary<uint, Pointer>();
        uint numActiveContacts=0;
        ViewModel.ColumnViewModel columnViewModel;
        private Point initialPoint;

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
            columnViewModel = this.DataContext as ViewModel.ColumnViewModel;
        }

        private void PointerIn(Pointer pointer)
        {
            if (contacts.ContainsKey(pointer.PointerId)) { return; }
            contacts[pointer.PointerId] = pointer;
            ++numActiveContacts;
        }

        private void PointerOut(Pointer pointer)
        {
            if (contacts.ContainsKey(pointer.PointerId))
            {
                contacts[pointer.PointerId] = null;
                contacts.Remove(pointer.PointerId);
                --numActiveContacts;
            }
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PointerIn(e.Pointer);
        }

        private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerIn(e.Pointer);
            initialPoint = e.GetCurrentPoint(this).Position;

            Down.IsOpen = true;
            Up.IsOpen = true;

            /*if (columnViewModel.Enabled)
            {
                Right.IsOpen = true;
                Left.IsOpen = false;
            }
            else
            {
                Left.IsOpen = true;
                Right.IsOpen = false;
            }*/
            

            /* initialize animation*/
            ShowDownStoryboard.Begin();
            ShowRightStoryboard.Begin();
            ShowLeftStoryboard.Begin();
            ShowUpStoryboard.Begin();
            HideDownStoryboard.Stop();
            HideRightStoryboard.Stop();
            HideUpStoryboard.Stop();
            HideLeftStoryboard.Stop();
            DownFocusTransform.Y = 0;
            UpFocusTransform.Y = 0;
            LeftFocusTransform.X = 0;
            RightFocusTransform.X = 0;

            //columnViewModel.Highlight();
            Wrapper.CapturePointer(e.Pointer);
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            PointerOut(e.Pointer);
        }

        private void Border_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            PointerOut(e.Pointer);
            if (numActiveContacts == 0)
            {
                HideDownStoryboard.Begin();
                HideRightStoryboard.Begin();
                HideLeftStoryboard.Begin();
                HideUpStoryboard.Begin();
                UnHighlightUpStoryboard.Begin();
                UnHighlightDownStoryboard.Begin();
                UnHighlightLeftStoryboard.Begin();
                UnHighlightRightStoryboard.Begin();

                if (upSelected)
                {
                    //columnViewModel.SortDescending();
                }
                else if (downSelected)
                {
                   // columnViewModel.SortAscending();
                }
                else if (leftSelected)
                {
                    //columnViewModel.MarkEnabled();
                }
                else if (rightSelected)
                {
                    //columnViewModel.MarkDisabled();
                }
                upSelected = downSelected = leftSelected = rightSelected = false;
                //columnViewModel.Unhighlight();
            }
        }

        private void Border_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            PointerOut(e.Pointer);
        }

        Boolean leftDirty = false, downDirty = false, upDirty = false, rightDirty = false; // 기본자리에서 움직였나?
        Boolean leftSelected = false, downSelected = false, upSelected = false, rightSelected = false; // 선택되어서 하이라이트되었나? 

        void Reset(String except)
        {
            if (except != "left" && leftDirty)
            {
                ResetLeftStoryboard.Begin();
                if(leftSelected) UnHighlightLeftStoryboard.Begin();
                leftDirty = false;
                leftSelected = false;
            }

            if (except != "right" && rightDirty)
            {
                ResetRightStoryboard.Begin();
                if (rightSelected) UnHighlightRightStoryboard.Begin();
                rightDirty = false;
                rightSelected = false;
            }

            if (except != "up" && upDirty)
            {
                ResetUpStoryboard.Begin();
                if (upSelected) UnHighlightUpStoryboard.Begin();
                upDirty = false;
                upSelected = false;
            }

            if (except != "down" && downDirty)
            {
                ResetDownStoryboard.Begin();
                if (downSelected) UnHighlightDownStoryboard.Begin();
                downDirty = false;
                downSelected = false;
            }
        }

        const Double HorizontalSelectThreshold = 15;
        const Double VerticalSelectThreshold = 15;
        const Double SnapThreshold = 40;

        private void Border_PointerMoved(object sender, PointerRoutedEventArgs e)
        {            
            Double y = e.GetCurrentPoint(this).Position.Y - initialPoint.Y,
                   x = e.GetCurrentPoint(this).Position.X - initialPoint.X;

            Debug.WriteLine("{0} {1}", x, y);

            if (Math.Abs(x) <= SnapThreshold)
                x = 0;
            else if (x >= 0)
                x -= SnapThreshold;
            else
                x += SnapThreshold;

            if (Math.Abs(y) <= SnapThreshold)
                y = 0;
            else if (y >= 0)
                y -= SnapThreshold;
            else
                y += SnapThreshold;

            if (x == 0 && y == 0)
            {
                Reset("all");
            }
            else if (-y >= x && -y >= -x) //up
            {
                Reset("up");

                if (y < -VerticalSelectThreshold)
                {
                    //UpFocusTransform.Y = VerticalSelectThreshold;
                    if (!upSelected) HighlightUpStoryboard.Begin();
                    upSelected = true;
                    upDirty = true;
                }
                else if (y < 0)
                {
                    //UpFocusTransform.Y = -y;
                    //upDirty = true;
                    if (upSelected) UnHighlightUpStoryboard.Begin();
                    upSelected = false;
                }
            }
            else if (y >= x && y >= -x) //down
            {
                Reset("down");

                if (y > VerticalSelectThreshold)
                {
                    //DownFocusTransform.Y = -VerticalSelectThreshold;
                    if (!downSelected) HighlightDownStoryboard.Begin();
                    downSelected = true;
                    downDirty = true;
                }
                else if (y > 0)
                {
                    //DownFocusTransform.Y = -y;
                    //downDirty = true;
                    if (downSelected) UnHighlightDownStoryboard.Begin();
                    downSelected = false;
                }
            }
            else if (x >= y && x >= -y) //right
            {
                Reset("right");

                if (x > HorizontalSelectThreshold)
                {
                    //RightFocusTransform.X = -HorizontalSelectThreshold;
                    rightDirty = true;
                    if (!rightSelected) HighlightRightStoryboard.Begin();
                    rightSelected = true;
                }
                else if (x > 0)
                {
                    //RightFocusTransform.X = -x;
                    //rightDirty = true;
                    if (rightSelected) UnHighlightRightStoryboard.Begin();
                    rightSelected = false;
                }
            }
            else //left
            {
                Reset("left");

                if (x < -HorizontalSelectThreshold)
                {
                    //LeftFocusTransform.X = HorizontalSelectThreshold;
                    if (!leftSelected) HighlightLeftStoryboard.Begin();
                    leftSelected = true;
                    leftDirty = true;
                }
                else if (x < 0)
                {
                    //LeftFocusTransform.X = -x;
                    //leftDirty = true;
                    if (leftSelected) UnHighlightLeftStoryboard.Begin();
                    leftSelected = false;
                }
            }
                

        }

        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Double y = e.Cumulative.Translation.Y,
                   x = e.Cumulative.Translation.X;

            if (Math.Abs(x) <= SnapThreshold)
                x = 0;
            else if (x >= 0)
                x -= SnapThreshold;
            else
                x += SnapThreshold;

            if (Math.Abs(y) <= SnapThreshold)
                y = 0;
            else if (y >= 0)
                y -= SnapThreshold;
            else
                y += SnapThreshold;

            if (x == 0 && y == 0)
            {
                Reset("all");
            }
            else if (-y >= x && -y >= -x) //up
            {
                Reset("up");

                if (y < -VerticalSelectThreshold)
                {
                    //UpFocusTransform.Y = VerticalSelectThreshold;
                    if (!upSelected) HighlightUpStoryboard.Begin();
                    upSelected = true;
                    upDirty = true;
                }
                else if (y < 0)
                {
                    //UpFocusTransform.Y = -y;
                    //upDirty = true;
                    if (upSelected) UnHighlightUpStoryboard.Begin();
                    upSelected = false;
                }
            }
            else if (y >= x && y >= -x) //down
            {
                Reset("down");

                if (y > VerticalSelectThreshold)
                {
                    //DownFocusTransform.Y = -VerticalSelectThreshold;
                    if (!downSelected) HighlightDownStoryboard.Begin();
                    downSelected = true;
                    downDirty = true;
                }
                else if(y > 0)
                {
                    //DownFocusTransform.Y = -y;
                    //downDirty = true;
                    if (downSelected) UnHighlightDownStoryboard.Begin();
                    downSelected = false;
                }
            }
            else if (x >= y && x >= -y) //right
            {
                Reset("right");

                if (x > HorizontalSelectThreshold)
                {
                    //RightFocusTransform.X = -HorizontalSelectThreshold;
                    rightDirty = true;
                    if (!rightSelected) HighlightRightStoryboard.Begin();
                    rightSelected = true;                    
                }
                else if (x > 0)
                {
                    //RightFocusTransform.X = -x;
                    //rightDirty = true;
                    if (rightSelected) UnHighlightRightStoryboard.Begin();
                    rightSelected = false;
                }
            }
            else //left
            {
                Reset("left");

                if (x < -HorizontalSelectThreshold)
                {
                    //LeftFocusTransform.X = HorizontalSelectThreshold;
                    if (!leftSelected) HighlightLeftStoryboard.Begin();
                    leftSelected = true;
                    leftDirty = true;
                }
                else if (x < 0)
                {
                    //LeftFocusTransform.X = -x;
                    //leftDirty = true;
                    if (leftSelected) UnHighlightLeftStoryboard.Begin();
                    leftSelected = false;
                }
            }
                
        }

        private void HideDownStoryboard_Completed(object sender, object e)
        {
            Down.IsOpen = false;
        }

        private void HideRightStoryboard_Completed(object sender, object e)
        {
            Right.IsOpen = false;
        }

        private void HideLeftStoryboard_Completed(object sender, object e)
        {
            Left.IsOpen = false;
        }

        private void HideUpStoryboard_Completed(object sender, object e)
        {
            Up.IsOpen = false;
        }
    }
}
