using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Controls.CommonDialog;
using ClassIsland.Core.Enums.SettingsWindow;
using ClassIsland.Shared.Helpers;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using ClassIsland.Core;
using System.Drawing.Printing;

namespace LXMusicPlugins
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    [SettingsPageInfo("lh11117.LXMusicPlugins", "LX Music 歌词插件", PackIconKind.Music, PackIconKind.Music, SettingsPageCategory.External)]
    public partial class SettingsPage : SettingsPageBase
    {
        Tools t = new();
        ConfigPath c = new();
        public Settings Settings { get; set; } = new();
        private bool SaveSettings = true;
        public SettingsPage()
        {
            InitializeComponent();

            Settings = ConfigureFileHelper.LoadConfig<Settings>(c.Get());  // 加载配置文件
            //Settings.PropertyChanged += (sender, args) =>
            //{
            //    ConfigureFileHelper.SaveConfig<Settings>(c.Get(), Settings);  // 保存配置文件
            //};

            SaveSettings = false;   // 巴哥警告()
            IP.Text = Settings.IP;
            Port.Text = Settings.Port;
            SecondTime.Text = Settings.SecondTime.ToString();
            PauseOnClass.IsChecked = Settings.PauseOnClass;
            SaveSettings = true;
            IP.TextChanged += WriteSettings;
            Port.TextChanged += WriteSettings;
            SecondTime.TextChanged += WriteSettings;
            PauseOnClass.Checked += WriteSettings2;
            PauseOnClass.Unchecked += WriteSettings2;
        }

        private void WriteSettings2(object sender, RoutedEventArgs e)
        {
            WriteSettings(null, null);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            String Url = "http://" + IP.Text + ":" + Port.Text + "/";
            String LRC = "";
            int time = 0;
            try
            {
                {
                    HttpClient client = new();
                    HttpResponseMessage response = await client.GetAsync(Url + "lyric");
                    response.EnsureSuccessStatusCode();
                    LRC = await response.Content.ReadAsStringAsync();
                }
                {
                    HttpClient client = new();
                    HttpResponseMessage response = await client.GetAsync(Url + "status");
                    response.EnsureSuccessStatusCode();
                    LX_Status s = JsonSerializer.Deserialize<LX_Status>(await response.Content.ReadAsStringAsync());
                    time = (int)(s.progress * 1000);
                    Console.WriteLine(time);
                }
                CommonDialog.ShowInfo("当前歌词: " + t.FindSong(t.LRC_Load(LRC), time) + "\n下一歌词: " + t.FindNextSong(t.LRC_Load(LRC), time));
            }
            catch (Exception)
            {
                CommonDialog.ShowError("错误! ");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("是否设置为默认设置? ", "LX Music Plugins for CI", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                IP.Text = "127.0.0.1";
                Port.Text = "23330";
            }
        }

        private void WriteSettings(object sender, TextChangedEventArgs e)
        {
            if (SaveSettings && IP != null && IP.Text != null && Port != null && Port.Text != null && SecondTime != null && SecondTime.Text != null && PauseOnClass != null && PauseOnClass.IsChecked != null)
            {
                Settings.IP = IP.Text;
                Settings.Port = Port.Text;
                Settings.PauseOnClass = (bool)PauseOnClass.IsChecked;
                try
                {
                    Settings.SecondTime = int.Parse(SecondTime.Text);
                }
                catch (Exception) { }
                ConfigureFileHelper.SaveConfig<Settings>(c.Get(), Settings);
                Console.WriteLine(c.Get());
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            RequestRestart();
        }
    }
}
