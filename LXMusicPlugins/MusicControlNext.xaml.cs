using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Shared.Helpers;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace LXMusicPlugins
{
    [ComponentInfo(
        "E2340F82-CB0D-4E2C-9D7C-8CB4DE671D45",
        "LX Music 歌词(下一歌词)",
        PackIconKind.CalendarOutline,
        "显示LX Music的歌词(下一歌词)"
    )]

    /// <summary>
    /// MusicControlNext.xaml 的交互逻辑
    /// </summary>
    public partial class MusicControlNext : ComponentBase
    {
        ConfigPath c = new();
        Tools t = new();
        public Settings Settings { get; set; } = new();
        System.Timers.Timer timer = new System.Timers.Timer();

        async private void StartGetLyric(object? sender, EventArgs? e)
        {
            Dictionary<int, string> NextLRC = null;
            try
            {
                var client = new HttpClient();
                using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post,
                    "http://" + Settings.IP + ":" + Settings.Port + "/subscribe-player-status?filter=progress,status,name");
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
                        //Console.WriteLine(buff.ToString());
                        SSE sse = t.SSE_Load(buff.ToString().Trim('\n'));
                        if (sse.Event == "name")
                        {    // 换歌了，刷新LRC
                            String LRC = "";
                            {
                                HttpClient Client = new();
                                HttpResponseMessage Response = await Client.GetAsync("http://" + Settings.IP + ":" + Settings.Port + "/lyric");
                                Response.EnsureSuccessStatusCode();
                                LRC = await Response.Content.ReadAsStringAsync();
                            }
                            NextLRC = t.LRC_Load(LRC);
                        }
                        else if (sse.Event == "progress")
                        {
                            if (NextLRC == null)
                            {
                                String LRC = "";
                                {
                                    HttpClient Client = new();
                                    HttpResponseMessage Response = await Client.GetAsync("http://" + Settings.IP + ":" + Settings.Port + "/lyric");
                                    Response.EnsureSuccessStatusCode();
                                    LRC = await Response.Content.ReadAsStringAsync();
                                }
                                NextLRC = t.LRC_Load(LRC);
                            }

                            if (sse.num != null) LxText.Text = t.FindNextSong(NextLRC, (int)(sse.num * 1000));
                            else LxText.Text = "获取失败";

                            if (Settings.SecondTime > 0) timer.Stop();
                        } else if (sse.Event == "status")
                        {
                            if (sse.str == "paused" || sse.str == "stoped")
                            {
                                Console.WriteLine("好机会！");
                                if (Settings.SecondTime > 0) timer.Start();
                                else if (Settings.SecondTime == 0) LxText.Text = "";
                            }
                        }
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

        public MusicControlNext()
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
