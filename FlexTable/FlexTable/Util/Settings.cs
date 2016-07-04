using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FlexTable.Util
{
    public class Settings
    {
        private static Settings instance;

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Settings();
                }
                return instance;
            }
        }

        private String recentFile = null;
        public String RecentFile
        {
            get { return recentFile; }
            set
            {
                recentFile = value;
                saveSetting("recentFile", value);
            }
        }

        private Boolean animationEnabled = true;
        public Boolean AnimationEnabled
        {
            get { return animationEnabled; }
            set
            {
                animationEnabled = value;
                saveSetting("animationEnabled", value);
            }
        }

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private Settings()
        {
            if (localSettings.Values.ContainsKey("recentFile")) recentFile = (String)localSettings.Values["recentFile"];

            if (localSettings.Values.ContainsKey("animationEnabled")) animationEnabled = (Boolean)localSettings.Values["animationEnabled"];
        }

        void saveSetting(String name, Object value)
        {
            localSettings.Values[name] = value;
        }
    }
}
