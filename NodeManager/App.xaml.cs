﻿using NodeManager.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using NodeManager.Helpers;
using System.Windows.Threading;
using Prism.Mvvm;
using NodeManager.ViewModels;

namespace NodeManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            InitHelper.PrepareAll();
        }

        // protected override void OnStartup(StartupEventArgs e)
        // {
        
        // }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            RaygunHelper.Send(e.Exception);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<AutoLoad>();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.Register(typeof(MainWindow).ToString(), typeof(MainWindowViewModel));
            ViewModelLocationProvider.Register(typeof(VersionManager).ToString(), typeof(VersionManagerViewModel));
        }
    }
}