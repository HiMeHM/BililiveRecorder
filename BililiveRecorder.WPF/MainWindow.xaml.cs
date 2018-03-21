﻿using BililiveRecorder.Core;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BililiveRecorder.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const int MAX_LOG_ROW = 25;

        public Recorder Recorder { get; set; }
        public ObservableCollection<string> Logs { get; set; } =
            new ObservableCollection<string>()
            {
                "注：按鼠标右键复制日志",
                "网站： https://rec.danmuji.org",
            };

        public static void AddLog(string message) => _AddLog?.Invoke(message);
        private static Action<string> _AddLog;

        public MainWindow()
        {
            _AddLog = (message) => { Logs.Add(message); while (Logs.Count > MAX_LOG_ROW) Logs.RemoveAt(0); };

            InitializeComponent();

            Recorder = new Recorder();
            DataContext = this;


        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            LoadRooms();
            Task.Run(() => CheckVersion());
        }

        private void CheckVersion()
        {
            UpdateBar.MainButtonClick += UpdateBar_MainButtonClick;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                ad.CheckForUpdateCompleted += Ad_CheckForUpdateCompleted;
                ad.CheckForUpdateAsync();
            }
        }

        private Action UpdateAction;
        private void UpdateBar_MainButtonClick(object sender, RoutedEventArgs e) => UpdateAction?.Invoke();

        private void Ad_CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
            if (e.Error != null)
            {
                logger.Error(e.Error, "检查版本更新出错");
                return;
            }
            if (e.Cancelled)
                return;
            if (e.UpdateAvailable)
            {
                if (e.IsUpdateRequired)
                {
                    BeginUpdate();
                }
                else
                {
                    UpdateAction = () => BeginUpdate();
                    UpdateBar.MainText = string.Format("发现新版本: {0} 大小: {1}KiB", e.AvailableVersion, e.UpdateSizeBytes / 1024);
                    UpdateBar.ButtonText = "下载更新";
                    UpdateBar.Display = true;
                }
            }
        }

        private void BeginUpdate()
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
            ad.UpdateCompleted += Ad_UpdateCompleted;
            ad.UpdateProgressChanged += Ad_UpdateProgressChanged;
            ad.UpdateAsync();
            UpdateBar.ProgressText = "0KiB / 0KiB - 0%";
            UpdateBar.Progress = 0;
            UpdateBar.Display = true;
            UpdateBar.ShowProgressBar = true;
        }

        private void Ad_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            UpdateBar.Progress = e.BytesCompleted / e.BytesTotal;
            UpdateBar.ProgressText = string.Format("{0}KiB / {1}KiB - {2}%", e.BytesCompleted / 1024, e.BytesTotal / 1024, e.BytesCompleted / e.BytesTotal);
        }

        private void Ad_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                UpdateBar.Display = false;
                return;
            }
            if (e.Error != null)
            {
                UpdateBar.Display = false;
                logger.Error(e.Error, "下载更新时出现错误");
                return;
            }

            UpdateAction = () =>
            {
                System.Windows.Forms.Application.Restart();
            };
            UpdateBar.MainText = "更新已下载好，要现在重启软件吗？";
            UpdateBar.ButtonText = "重启软件";
            UpdateBar.ShowProgressBar = false;
        }

        private void LoadSettings()
        {
            // TODO: Load Settings
        }

        private void LoadRooms()
        {
            Recorder.AddRoom(12345);
        }

        private void TextBlock_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void RoomList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
