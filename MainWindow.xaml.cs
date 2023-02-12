using System;
using System.Collections.Generic;
using System.Linq;
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
using Panuon.UI.Silver;
using SquareMinecraftLauncher;
using SquareMinecraftLauncherWPF;

namespace LwMinecraftLauncher
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowX
    {
        LoginUI.Offline Offline = new LoginUI.Offline();
        LoginUI.Mojang Mojang = new LoginUI.Mojang();
        microsoft_launcher.MicrosoftAPIs microsoftAPIs = new microsoft_launcher.MicrosoftAPIs();
        SquareMinecraftLauncher.Minecraft.Game game = new SquareMinecraftLauncher.Minecraft.Game();
        LoginUI.Microsoft Microsoft = new LoginUI.Microsoft();
        public int launchMode;
        SquareMinecraftLauncher.Minecraft.Tools tools = new SquareMinecraftLauncher.Minecraft.Tools();
        
        public MainWindow()
        {
            InitializeComponent();
            //自动找版本
            var versions = tools.GetAllTheExistingVersion();
            versionCombo.ItemsSource = versions;
            //自动找java
            List<string> javaList = new List<string>();
            javaList.Add(tools.GetJavaPath());
            javaCombo.ItemsSource = javaList;
            //初始选择
            versionCombo.SelectedItem = versionCombo.Items[0];
            ContentControl.Content = new Frame
            {
                Content = Offline
            };
            launchMode = 1;
        }
        public async void GameStart()
        {
            try
            {
                if (versionCombo.Text != string.Empty && javaCombo.Text != string.Empty && MemoryTextbox.Text != string.Empty &&
                    (Offline.IdTextbox.Text != string.Empty || Mojang.Email.Text != string.Empty || Mojang.Password.Password != string.Empty))
                {
                    switch (launchMode)
                    {
                        case 1:
                            await game.StartGame(versionCombo.Text,@"D:\Java\java8\bin\javaw.exe", Convert.ToInt32(MemoryTextbox.Text), Offline.IdTextbox.Text);
                            break;
                        case 2:
                            await game.StartGame(versionCombo.Text, javaCombo.Text, Convert.ToInt32(MemoryTextbox.Text), Mojang.Email.Text, Mojang.Password.Password);
                            break;
                        case 3:
                            microsoft_launcher.MicrosoftAPIs microsoftAPIs = new microsoft_launcher.MicrosoftAPIs();
                            var v = Microsoft.MicrosoftWebBrowser.Source.ToString().Replace(microsoftAPIs.cutUri, string.Empty);
                            var t = Task.Run(() =>
                            {
                                return microsoftAPIs.GetAccessTokenAsync(v, false).Result;
                            });
                            await t;
                            var v1 = microsoftAPIs.GetAllThings(t.Result.access_token, false);
                            await game.StartGame(versionCombo.Text, javaCombo.Text, Convert.ToInt32(MemoryTextbox.Text), v1.name, v1.uuid, v1.mcToken, string.Empty, string.Empty);
                            break;
                    }
                }
                else
                {
                    MessageBoxX.Show("信息未填充完整", "错误");
                }
            }
            catch
            {
                MessageBoxX.Show("启动失败", "错误");
            }
        }
    
        //启动按钮
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GameStart();
        }

        //离线登录
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Frame
            {
                Content = Offline
            };
            launchMode = 1;
        }

        //Mojang登录
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Frame
            {
                Content = Mojang
            };
            launchMode = 2;
        }

        //微软登录
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Frame
            {
                Content = Microsoft
            };
            launchMode = 3;
        }
    }
}
