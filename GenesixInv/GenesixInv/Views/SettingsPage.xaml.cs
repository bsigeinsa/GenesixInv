using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using GenesixInv.Models;
using GenesixInv.Helper;
using GenesixInv.Services;
using System.IO;
using Xamarin.Essentials;
using GenesixInv.Resources;

namespace GenesixInv.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public Settings Setting { get; set; }

        public SettingsPage()
        {
            InitializeComponent();
            Setting = App.GnxSettings;

            BindingContext = this;
        }
        async void btnDownload_Clicked(System.Object sender, System.EventArgs e)
        {
            var res = await GnxHsh.downloadArticulos();
            if (!res)
            {
                await DisplayAlert("Error", "Problemas al cargar el archivo de artículos", "Aceptar");

            }
            else
            {
                await DisplayAlert("", "Fichero hash de artículos cargado correctamente", "Aceptar");

            }

        }

        async void btnUpload_Clicked(System.Object sender, System.EventArgs e)
        {
            var send = new RestRespuesta();
            var ser = new RestService();
            int count = 0;
            string fileName = Path.Combine(FileSystem.AppDataDirectory, App.GnxSettings.ficheroconteo + "1.txt");
            if (File.Exists(fileName))
            {
                send.file = fileName;
                send.fileContent = File.ReadAllText(fileName);
                var resp = await ser.DescargarAsync(send);
                if (resp.status == "ok")
                {
                    File.Delete(fileName);
                    count++;
                }
            }
            fileName = Path.Combine(FileSystem.AppDataDirectory, App.GnxSettings.ficheroconteo + "2.txt");
            if (File.Exists(fileName))
            {
                send.file = fileName;
                send.fileContent = File.ReadAllText(fileName);
                var resp = await ser.DescargarAsync(send);
                if (resp.status == "ok")
                {
                    File.Delete(fileName);
                    count++;
                }
            }
            if (count > 0)
            {
                await DisplayAlert("", "Se han descargado " + count.ToString() + " ficheros de conteo correctamente", AppResources.Aceptar);
                var shellI = (AppShell.Current.Items[0].Items[0] as IShellSectionController).PresentedPage;
                var inv = (Inventario)shellI;
                inv.ResetViewModel();

            }
            else
            {
                await DisplayAlert("Error", "No se ha descargado ningún fichero", AppResources.Aceptar);

            }
        }
    }
}