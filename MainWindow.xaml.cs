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
using KMCCC.Launcher;
using KMCCC.Authentication;
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
        LoginUI.Microsoft Microsoft = new LoginUI.Microsoft();
        public int launchMode;
        SquareMinecraftLauncher.Minecraft.Tools tools = new SquareMinecraftLauncher.Minecraft.Tools();
        public static LauncherCore Core = LauncherCore.Create();
        public MainWindow()
        {
            InitializeComponent();
            //自动找版本
            var versions = Core.GetVersions().ToArray();
            versionCombo.ItemsSource = versions;
            //自动找java
            List<string> javaList = new List<string>();
            foreach (string i in KMCCC.Tools.SystemTools.FindJava())
            {
                javaList.Add(i);
            }
            javaList.Add(tools.GetJavaPath());
            javaCombo.ItemsSource = javaList;
            //初始选择
            versionCombo.SelectedItem = versionCombo.Items[0];
            javaCombo.SelectedItem = javaCombo.Items[0];
            ContentControl.Content = new Frame
            {
                Content = Offline
            };
            launchMode = 1;
        }
        public async void GameStart()
        {
            LaunchOptions launchOptions = new LaunchOptions();
            switch (launchMode)
            {
                case 1:
                    launchOptions.Authenticator = new OfflineAuthenticator(Offline.IdTextbox.Text);
                    break;
                case 2:
                    launchOptions.Authenticator = new YggdrasilLogin(Mojang.Email.Text, Mojang.Password.Password,false);
                    break;
            }
            launchOptions.MaxMemory = Convert.ToInt32(MemoryTextbox.Text);
            if (versionCombo.Text != string.Empty&&javaCombo.Text != string.Empty&&MemoryTextbox.Text != string.Empty&&
                (Offline.IdTextbox.Text != string.Empty||Mojang.Email.Text != string.Empty||Mojang.Password.Password != string.Empty))
            {
                try
                {
                    if(launchMode != 3)
                    {
                        Core.JavaPath = javaCombo.Text;
                        var ver = (KMCCC.Launcher.Version)versionCombo.SelectedItem;
                        launchOptions.Version = ver;
                        var result = Core.Launch(launchOptions);
                        if (!result.Success)
                        {
                            switch (result.ErrorType)
                            {
                                case ErrorType.NoJAVA:
                                    MessageBoxX.Show("Java错误，详细信息：" + result.ErrorMessage, "错误");
                                    break;
                                case ErrorType.AuthenticationFailed:
                                    MessageBoxX.Show("登录错误，详细信息：" + result.ErrorMessage, "错误");
                                    break;
                                case ErrorType.UncompressingFailed:
                                    MessageBoxX.Show("文件错误，详细信息：" + result.ErrorMessage, "错误");
                                    break;
                                default:
                                    MessageBoxX.Show("未知错误，详细信息：" + result.ErrorMessage, "错误");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        microsoft_launcher.MicrosoftAPIs microsoftAPIs = new microsoft_launcher.MicrosoftAPIs();
                        var v = Microsoft.MicrosoftWebBrowser.Source.ToString().Replace(microsoftAPIs.cutUri,string.Empty);
                        var t = Task.Run(() =>
                        {
                            return microsoftAPIs.GetAccessTokenAsync(v, false).Result;
                        });
                        await t;
                        var v1 = microsoftAPIs.GetAllThings(t.Result.access_token, false);
                        SquareMinecraftLauncher.Minecraft.Game game = new SquareMinecraftLauncher.Minecraft.Game();
                        await game.StartGame(versionCombo.Text,javaCombo.Text,Convert.ToInt32(MemoryTextbox.Text),v1.name,v1.uuid,v1.mcToken,string.Empty,string.Empty);
                    }
                }
                catch
                {
                    MessageBoxX.Show("启动失败", "错误");
                }
            }
            else
            {
                MessageBoxX.Show("信息未填充完整", "错误");
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
