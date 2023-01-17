using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using GenesixInv.Services;
using GenesixInv.Views;
using GenesixInv.Models;

namespace GenesixInv
{
    public partial class App : Application
    {
        public static Settings GnxSettings { get; set; }
        public static RestRespuesta LastResp { get; set; }
        public static AppShell myshell { get; set; }

        public App ()
        {
            InitializeComponent();
            GnxSettings = new Settings();

            MainPage = new AppShell();
            myshell = (AppShell)MainPage;

        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}

