using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Microsoft.Win32;
using Newtonsoft.Json;
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
        //初始化
        LoginUI.Offline Offline = new LoginUI.Offline();
        LoginUI.Mojang Mojang = new LoginUI.Mojang();
        microsoft_launcher.MicrosoftAPIs microsoftAPIs = new microsoft_launcher.MicrosoftAPIs();
        SquareMinecraftLauncher.Minecraft.Game game = new SquareMinecraftLauncher.Minecraft.Game();
        LoginUI.Microsoft Microsoft = new LoginUI.Microsoft();
        public int launchMode;
        SquareMinecraftLauncher.Minecraft.Tools tools = new SquareMinecraftLauncher.Minecraft.Tools();
        string settingPath = @"./LMCL/LwMinecraftLauncher.json";
        Setting setting = new Setting();
        RegisterSetting registerSetting = new RegisterSetting();
        string Access_token;

        //数据保存
        public class Setting
        {
            public string Ram = "1024";
        }
        public class RegisterSetting
        {
            public string name = string.Empty;
            public string email = string.Empty;
            public string password = string.Empty;
        }

        public void LauncherInitialization()
        {
            if (!File.Exists(settingPath))
            {
                Directory.CreateDirectory(@"./LMCL");
                File.WriteAllText(settingPath, JsonConvert.SerializeObject(setting));
            }
            else
            {
                setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(settingPath));
            }
            bool isFirst = true;
            using(RegistryKey key1 = Registry.LocalMachine.OpenSubKey("Software"))
            {
                foreach(var i in key1.GetSubKeyNames())
                {
                    if(i == "LwMinecraftLauncherSetting")
                    {
                        isFirst = false;
                    }
                }
            }
            if (isFirst)
            {
                using (RegistryKey key = Registry.LocalMachine)
                {
                    using (RegistryKey software = key.CreateSubKey("software\\LwMinecraftLauncherSetting"))
                    {
                        software.SetValue("name", registerSetting.name);
                        software.SetValue("email", registerSetting.email);
                        software.SetValue("password", registerSetting.password);
                    }
                }
            }
            else
            {
                using (RegistryKey key = Registry.LocalMachine)
                {
                    using (RegistryKey software = key.CreateSubKey("software\\LwMinecraftLauncherSetting"))
                    {
                        registerSetting.name = software.GetValue("name").ToString();
                        registerSetting.email = software.GetValue("email").ToString();
                        registerSetting.password = software.GetValue("password").ToString();
                    }
                }
            }
            Offline.IdTextbox.Text = registerSetting.name;
            Mojang.Email.Text = registerSetting.email;
            Mojang.Password.Password = registerSetting.password;
            //自动找版本
            var versions = tools.GetAllTheExistingVersion();
            versionCombo.ItemsSource = versions;
            //自动找java
            List<string> javaList = new List<string>();
            javaList.Add(tools.GetJavaPath());
            foreach (var i in microsoftAPIs.GetJavaInstallationPath())
            {
                javaList.Add(i.Path.ToString());
            }
            javaCombo.ItemsSource = javaList;
            //初始选择
            javaCombo.SelectedItem = javaCombo.Items[0];
            versionCombo.SelectedItem = versionCombo.Items[0];
            ContentControl.Content = new Frame
            {
                Content = Offline
            };
            launchMode = 1;
            MemoryTextbox.Text = setting.Ram;
        }

        public MainWindow()
        {
            InitializeComponent();
            LauncherInitialization();
            ServicePointManager.DefaultConnectionLimit = 512;
            
        }

        //游戏启动方法
        public async void GameStart()
        {
            try
            {
                //检查是否有空缺
                if (versionCombo.Text != string.Empty && javaCombo.Text != string.Empty && MemoryTextbox.Text != string.Empty &&
                    (Offline.IdTextbox.Text != string.Empty || Mojang.Email.Text != string.Empty || Mojang.Password.Password != string.Empty))
                {
                    switch (launchMode)
                    {
                        //离线启动
                        case 1:
                            await game.StartGame(versionCombo.Text, javaCombo.Text, Convert.ToInt32(MemoryTextbox.Text), Offline.IdTextbox.Text);
                            break;
                        //mojang启动
                        case 2:
                            await game.StartGame(versionCombo.Text, javaCombo.Text, Convert.ToInt32(MemoryTextbox.Text), Mojang.Email.Text, Mojang.Password.Password);
                            break;
                        //微软启动
                        case 3:
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

        //离线登录按钮
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Frame
            {
                Content = Offline
            };
            launchMode = 1;
        }

        //Mojang登录按钮
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Frame
            {
                Content = Mojang
            };
            launchMode = 2;
        }

        //微软登录按扭
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Frame
            {
                Content = Microsoft
            };
            launchMode = 3;
        }

        private void MemoryTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            setting.Ram = MemoryTextbox.Text;
        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            File.WriteAllText(settingPath, JsonConvert.SerializeObject(setting));
            using (RegistryKey key = Registry.LocalMachine)
            {
                using (RegistryKey software = key.CreateSubKey("software\\LwMinecraftLauncherSetting"))
                {
                    software.SetValue("name", Offline.IdTextbox.Text);
                    software.SetValue("email", Mojang.Email.Text);
                    software.SetValue("password", Mojang.Password.Password);
                }
            }
        }
    }
}
