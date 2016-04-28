using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FlexTable.Util
{
    public class Logger
    {
        private static Logger instance;

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
                return instance;
            }
        }

        StorageFile logFile;
        String buffer = "";
        Int32 bufferCount = 0;

        private Logger() { }
        
        public async Task Initialize()
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

        public async void Log(String log)
        {
            String withTime = $"{DateTime.Now.ToString("MMM월 d일 H시 mm분 ss초")},{log}\r\n";
            buffer += withTime;
            bufferCount++;

            if(bufferCount > 10)
            {
                try
                {
                    await FileIO.AppendTextAsync(logFile, buffer);
                }
                catch (Exception e)
                {

                }
                bufferCount = 0;
                buffer = "";
            }
        }

        public async Task Flush()
        {
            if (bufferCount > 0)
            {
                try
                {
                    await FileIO.AppendTextAsync(logFile, buffer);
                }
                catch (Exception e)
                {

                }
                buffer = "";
                bufferCount = 0;
            }
        }
    }
}
