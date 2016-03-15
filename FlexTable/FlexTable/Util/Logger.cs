using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FlexTable.Util
{
    public static class Logger
    {
        static StorageFile logFile;

        public static async Task Initialize()
        {
            StorageFolder documentLibrary = KnownFolders.DocumentsLibrary;
            StorageFolder logDirectory = null;

            if (await documentLibrary.TryGetItemAsync("FlexTableLogs") == null)
            {
                logDirectory = await documentLibrary.CreateFolderAsync("FlexTableLogs");
            }
            else
            {
                logDirectory = await documentLibrary.GetFolderAsync("FlexTableLogs");
            }

            logFile = await logDirectory.CreateFileAsync(DateTime.Now.ToString("MMM월 d일 H시 mm분 ss초") + ".txt");

        }

        public static async void Log(String log)
        {
            try { 
                String withTime = $"{DateTime.Now.ToString("MMM월 d일 H시 mm분 ss초")},{log}\r\n";
                await FileIO.AppendTextAsync(logFile, withTime);
            }
            catch
            {

            }
        }
    }
}
