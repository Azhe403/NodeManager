﻿using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using myoddweb.directorywatcher;
using myoddweb.directorywatcher.interfaces;
using NodeManager.Helpers;
using NodeManager.Views;
using Prism.Mvvm;
using Prism.Regions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NodeManager.Models;
using Prism.Commands;
using System.Windows;

namespace NodeManager.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Node Version Manager";
        private List<string> listLogs;
        private int _selectedLine;
        private WindowState _currentWindowState;
        private Stopwatch _stopwatchLogs;
        private HamburgerMenuItemCollection _hamburgerMenuItemCollection;

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public HamburgerMenuItemCollection HamburgerMenuItemCollection
        {
            get => _hamburgerMenuItemCollection;
            set => SetProperty(ref _hamburgerMenuItemCollection, value);
        }

        public int SelectedLine
        {
            get => _selectedLine;
            set => SetProperty(ref _selectedLine, value);
        }

        public List<string> ListLogs
        {
            get => listLogs;
            set => SetProperty(ref listLogs, value);
        }

        public WindowState CurrentWindowState
        {
            get => _currentWindowState;
            set => SetProperty(ref _currentWindowState, value);
        }

        public DelegateCommand OnDoubleClickTrayCommand { get; set; }

        //public MainWindowViewModel()
        //{
        //}

        public MainWindowViewModel()
        {
            // IRegionManager regionManager

            BuildMenuItems();

            if (EnvHelper.IsAdministrator())
            {
                Title += " (Administrator)";
            }

            OnDoubleClickTrayCommand = new DelegateCommand(OnDoubleClickTray);

            if (!Environment.Is64BitOperatingSystem)
            {
                var osWarn = "This is Windows 32-bit operating system. Currently NodeManager support for x64 Only.";
                DialogHelper.WarnDialog(osWarn,"OS architecture problem");

                Application.Current.Shutdown(0);
            }

            // PrismHelper.RegionManager = regionManager;
            _stopwatchLogs = new Stopwatch();

            Parallel.Invoke(async () => await LoadLogsAsync().ConfigureAwait(false));

            LogsWatcher();
        }

        private void OnDoubleClickTray()
        {
        }

        private void BuildMenuItems()
        {
            HamburgerMenuItemCollection = new HamburgerMenuItemCollection()
            {
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterialLight() {Kind = PackIconMaterialLightKind.Download},
                    Label = "Version Manager",
                    ToolTip = "Version Manager",
                    Tag = new VersionManager()
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterialLight() {Kind = PackIconMaterialLightKind.Repeat},
                    Label = "Package Manager",
                    ToolTip = "Package Manager",
                    Tag = new PackageManager()
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconFeatherIcons() {Kind = PackIconFeatherIconsKind.Settings},
                    Label = "Settings",
                    ToolTip = "Settings",
                    Tag = new Settings()
                }
            };
        }

        private void LogsWatcher()
        {
            // create Watcher
            var watch = new Watcher();

            // Add a request.
            watch.Add(new Request(AppConfig.LogsPath, true));

            watch.OnTouchedAsync += Watch_OnTouchedAsync;
            ;

            // start watching
            watch.Start();
            _stopwatchLogs.Start();
        }

        private async Task Watch_OnTouchedAsync(IFileSystemEvent e, CancellationToken token)
        {
            await Task.Run(async () =>
            {
                if (_stopwatchLogs.Elapsed.Seconds >= 1)
                {
                    _stopwatchLogs.Stop();
                    await LoadLogsAsync().ConfigureAwait(false);
                    _stopwatchLogs.Start();
                }
            }, token).ConfigureAwait(false);
        }

        private async Task LoadLogsAsync()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var logPath = Path.Combine(AppConfig.LogsPath, $"App-{date}.log");

            ListLogs = new List<string>();
            if (!logPath.IsFileExist()) return;

            using (FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
            {
                var logs = await sr.ReadToEndAsync().ConfigureAwait(false);
                ListLogs.AddRange(logs.Lines().Reverse());
            }

            ListLogs = ListLogs.Where(x => x.Contains("[INF]")).ToList();

            SelectedLine = 0;
        }
    }
}