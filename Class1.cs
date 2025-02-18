using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace chaosaddon
{
    public static class BPMData
    {
        
        
        private static Dictionary<string, int> BPM = new Dictionary<string, int>();
        static StreamReader sw = new StreamReader("BPM3.txt");
        
        public static void initalize()
        {
            string buffer = "";
            double bpm = 0;
            string name = "";
            while (!sw.EndOfStream)
            {
                try
                {
                    buffer = sw.ReadLine();
                    bpm = Convert.ToDouble(buffer.Substring(buffer.IndexOf('_') + 1));
                    name = buffer.Substring(0, buffer.IndexOf("  "));
                    bpm = 60000 / bpm;
                    //Console.WriteLine(bpm + " " + name);
                    BPM.Add(name, (int)bpm);
                }
                catch (Exception e)
                {
                    try
                    {
                       BPM.Add(name, -1);
                    }
                    catch ( Exception e2 ) 
                    {
                        
                    }
                }
            }

        }

        public static Dictionary<string, int> getData()
        {
            return BPM;
        }
        
    }
}
