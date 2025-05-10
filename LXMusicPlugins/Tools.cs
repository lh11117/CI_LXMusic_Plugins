using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace LXMusicPlugins
{
    public class LX_Status
    {
        public String? status { get; set; }
        public float progress { get; set; }
    }

    public class SSE {
        public String? Event { get; set; }
        public String? str { get; set; }
        public float? num { get; set; }
    }

    public class Tools
    {
        public Dictionary<int, String> LRC_Load(String _LRC)
        {
            Dictionary<int, String> ls = new();
            String[] LRCs = _LRC.Split("\n");
            foreach (String LRC in LRCs)
            {
                if (LRC == "") continue;
                String time = LRC.Split("]")[0].TrimStart('[');
                int t = Time2int(time);
                String song = LRC.Substring(time.Length + 2, LRC.Length - time.Length - 2);
                ls[t] = song;
            }
            return ls;
        }

        public int Time2int(String time)
        {
            int min, s, ms;
            try
            {
                min = int.Parse(time.Split(":")[0]);
                s = int.Parse(time.Split(":")[1].Split(".")[0]);
                ms = int.Parse(time.Split(".")[1]);
            }
            catch (Exception) { return 0; }
            return (min * 60 + s) * 1000 + ms;
        }

        public String FindSong(Dictionary<int, String> songs, int time)
        {
            String last = "";
            foreach(var v in songs)
            {
                if (v.Key > time)
                {
                    return last;
                }
                last = v.Value;
            }
            return last;
        }

        public String FindNextSong(Dictionary<int, String> songs, int time)
        {
            foreach(var v in songs)
            {
                if (v.Key > time)
                {
                    return v.Value;
                }
            }
            return "暂无歌词";
        }

        public SSE SSE_Load(String s)
        { 
            //Console.WriteLine("=="+s+"=="); 
            SSE sse = new();
            String a = s.Split("\n")[0], b = s.Split("\n")[1];
            sse.Event = a.Split(":")[1].Trim();
            String t = b.Split(":")[1].Trim();
            if (t.EndsWith("\"") && t.StartsWith("\""))
            {
                sse.str = t.Substring(1, t.Length - 2);
            } else
            {
                try
                {
                    sse.num = float.Parse(t);
                }
                catch (Exception) { sse.num = 0;}
            }
            return sse;
        }
    }
}
