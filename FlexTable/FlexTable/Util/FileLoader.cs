using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace FlexTable.Util
{
    class FileLoader
    {
        private static FileLoader instance;

        public static FileLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FileLoader();
                }
                return instance;
            }
        }

        private FileLoader() { }

        public async Task<StorageFile> Open()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".csv");
            fileOpenPicker.ViewMode = PickerViewMode.List;
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            return file;
        }
    }
}
