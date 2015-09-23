using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace FlexTable.Util
{
    public class HtmlToTextBlockFormatter
    {
        public static void Format(String html, TextBlock textBlock)
        {
            textBlock.Inlines.Clear();
              
            foreach(String a in html.Split(new String[] { "<b>" }, StringSplitOptions.RemoveEmptyEntries))
            {
                String[] result = a.Split(new String[] { "</b>" }, StringSplitOptions.None);

                if (result.Count() == 1)
                {
                    textBlock.Inlines.Add(new Run() { Text = result[0] });
                }
                else if (result.Count() == 2)
                {
                    textBlock.Inlines.Add(new Run() { Text = result[0], FontWeight = FontWeights.Bold });
                    textBlock.Inlines.Add(new Run() { Text = result[1] });
                }
                else
                {
                    throw new Exception("Wrong tag " + html);
                }
            }
        }
    }
}
