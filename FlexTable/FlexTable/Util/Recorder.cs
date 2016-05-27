using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Util
{
    class Recorder
    {
        private static Recorder instance;

        public static Recorder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Recorder();
                }
                return instance;
            }
        }

        Stopwatch watch = new Stopwatch();
        
        public void Start()
        {
            watch.Restart();
        }

        public void StopAndGo(String name)
        {
            Debug.WriteLine($"{name} {watch.ElapsedMilliseconds.ToString()}");
            watch.Restart();
        }
    }
}
