//#define a
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Shared.Helpers;
using ClassIsland.Shared.IPC;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YamlDotNet.Core;
using static unvell.Common.Win32Lib.Win32;

namespace LXMusicPlugins
{
    [ComponentInfo(
        "21F7252B-43D7-4D1D-A833-6F74D5B9D91D",
        "LX Music 歌词",
        PackIconKind.CalendarOutline,
        "显示LX Music的歌词"
    )]

    /// <summary>
    /// MusicControl.xaml 的交互逻辑
    /// </summary>
    public partial class MusicControl : ComponentBase
    {
        ConfigPath c = new();
        Tools t = new();
        System.Timers.Timer timer = new System.Timers.Timer();
        public Settings Settings { get; set; } = new();

        async private void StartGetLyric(object? sender, EventArgs? e)
        {
            try
            {
                var client = new HttpClient();
                using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post,
                    "http://" + Settings.IP + ":" + Settings.Port + "/subscribe-player-status?filter=lyricLineText,status");
                var response = await client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
                await using var stream = await response.Content.ReadAsStreamAsync();
                var streamReader = new System.IO.StreamReader(stream);
                var buffer = new Memory<char>(new char[1]);
                int writeLength = 0;
                String buff = "";
                while ((writeLength = await streamReader.ReadBlockAsync(buffer)) > 0)
                {
                    if (writeLength < buffer.Length)
                    {
                        buffer = buffer[..writeLength];
                    }
                    buff += buffer.ToString();
                    if (buff.EndsWith("\n\n"))
                    {
#if a
                    try
                    {
#endif
                        Console.WriteLine(buff.ToString());
                        SSE sse = t.SSE_Load(buff.ToString().Trim('\n'));
                        Console.WriteLine("--event: " + sse.Event + "  --str: "+sse.str);
                        if (sse.Event == "lyricLineText")
                        {
                            LxText.Text = sse.str;
                            if (Settings.SecondTime > 0) timer.Stop();
                        }
                        else if (sse.Event == "status")
                        {
                            if (sse.str == "paused" || sse.str == "stoped")
                            {
                                Console.WriteLine("好机会！");
                                if (Settings.SecondTime > 0) timer.Start();
                                else if (Settings.SecondTime == 0) LxText.Text = "";
                            }
                        }
                        //Console.WriteLine("获取歌词: "+sse.str);
#if a
                    }
                    catch (Exception) { }
#endif
                        buff = "";
                    }
                }
            }
            catch (Exception)
            {
                for (int i = 5; i > 0; i--)
                {
                    LxText.Text = "⚠获取出错, " + i + "s后重试⚠";
                    await Task.Delay(1000);
                }
                LxText.Text = "🚀正在重试...🚀";
                StartGetLyric(sender, e);
                return;
            }
        }

        public MusicControl()
        {
            InitializeComponent();

            Settings = ConfigureFileHelper.LoadConfig<Settings>(c.Get());

            if (Settings.SecondTime > 0)
            {
                timer.Interval = Settings.SecondTime;
                timer.AutoReset = false;
                timer.Enabled = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler((o, e) =>
                {
                    Console.WriteLine("就是现在！！！");
                    this.Invoke(new Action(() => {
                       LxText.Text = "";
                    }));
                });
            }
            else timer.Enabled = false;

            var app = AppBase.Current;

            app.AppStarted += StartGetLyric;
        }
    }
}
