using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ClassIsland.Shared.Helpers;
using ClassIsland.Core.Controls.CommonDialog;
using ClassIsland.Core.Extensions.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using Google.Protobuf.WellKnownTypes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Net.Http;
using ClassIsland.Shared;

namespace LXMusicPlugins
{
    public class Settings: ObservableObject
    {
        public String IP { get; set; } = "127.0.0.1";
        public String Port { get; set; } = "23330";
        public int SecondTime { get; set; } = -1;
        public bool PauseOnClass { get; set; } = false;
    }

    public class ConfigPath
    {
        public static String _ConfigPath { get; set; } = "";

        public String Get()
        {
            return _ConfigPath;
        }

        public void Set(String s)
        {
            _ConfigPath = s;
        }
    }

    [PluginEntrance]
    public class Plugin : PluginBase
    {
        public Settings Settings { get; set; } = new();
        public override void Initialize(HostBuilderContext context, IServiceCollection services)
        {
            services.AddComponent<MusicControl>();
            services.AddComponent<MusicControlNext>();
            services.AddSettingsPage<SettingsPage>();

            ConfigPath c = new();
            c.Set(Path.Combine(PluginConfigFolder, "Settings.json"));

            Settings = ConfigureFileHelper.LoadConfig<Settings>(c.Get());  // 加载配置文件
            Settings.PropertyChanged += (sender, args) =>
            {
                ConfigureFileHelper.SaveConfig<Settings>(c.Get(), Settings);  // 保存配置文件
            };

            /// todo: 修复下面触发不了的bug
            //ILessonsService ser = IAppHost.GetService<ILessonsService>();
            //ser.OnClass += async (o, e) =>
            //{
            //    Console.WriteLine("------------------------------------------触发");
            //    if (Settings.PauseOnClass)
            //    {
            //        HttpClient Client = new();
            //        HttpResponseMessage Response = await Client.GetAsync("http://" + Settings.IP + ":" + Settings.Port + "/pause");
            //        Response.EnsureSuccessStatusCode();
            //        await Response.Content.ReadAsStringAsync();
            //    }
            //};
        }
    }
}
