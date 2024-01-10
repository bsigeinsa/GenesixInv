using System;
using System.Collections.Generic;

using Xamarin.Forms;
using GenesixInv.Helper;
using GenesixInv.ViewModels;
using GenesixInv.Models;
using GenesixInv.Resources;
using System.Globalization;
using System.IO;
using Xamarin.Essentials;

namespace GenesixInv.Views
{
    public partial class Inventario : ContentPage
    {
        InventarioViewModel viewModel;
        public Inventario()
        {
            InitializeComponent();
            viewModel = new InventarioViewModel();
            BindingContext = viewModel;
        }
        public void ResetViewModel()
        {
            viewModel = new InventarioViewModel();
            BindingContext = viewModel;
            Tienda.Focus();
            newPlu.IsEnabled = false;

        }
        async void newPlu_Completed(System.Object sender, System.EventArgs e)
        {
            if(!string.IsNullOrEmpty( newPlu.Text))
            {
                //Primero miramos la mascara de pesable y que sea un posible ean 128
                string cod = newPlu.Text;
                Ean128 nean = new Ean128();
                if (nean.Parse(cod, ' ', App.GnxSettings.mascara, true))
                {
                    if (nean.peso == 0)
                    {
                        nean.Parse(cod, ' ', App.GnxSettings.mascara1, true);
                        if (nean.peso == 0)
                        {
                            nean.Parse(cod, ' ', App.GnxSettings.mascara2, true);
                        }
                    }
                    if (!string.IsNullOrEmpty(nean.Arti))
                    {
                        cod = nean.Arti;
                    }
                }
                GnxHsh hsh = new GnxHsh();
                string buf = "";
                if (hsh.gethsh("articulos.hsh", ref buf, cod.PadLeft(16), 16, 204) > 0)
                {
                    var larti = new Arti();
                    larti.setGnxMsg(buf);
                    viewModel.arti = larti;
                    if (nean.peso != 0)
                    {
                        viewModel.cantidad = (nean.peso / 1000).ToString("####.000",CultureInfo.CurrentCulture);
                        Total.Text =  (nean.peso / 1000).ToString("####.000", CultureInfo.CurrentCulture);
                        Cantidad_Completed(null, null);
                        return;
                    }
                    Cantidad.Text = "1";
                    Cantidad.IsEnabled = true;
                    Cantidad.Focus();
                }
                else
                {
                    await DisplayAlert(AppResources.Error, AppResources.ErrorPlu,AppResources.Aceptar);
                    newPlu.Text = "";
                    Cantidad.IsEnabled = false;
                    newPlu.Focus();
                }
            }
        }

        async void Zona_Completed(System.Object sender, System.EventArgs e)
        {
            if (Zona.Text.Length > 12)
            {
                await DisplayAlert(AppResources.Error, AppResources.ErrorZona, AppResources.Aceptar);
                Zona.Text = "";
                Zona.Focus();
                return;
            }

            if (!string.IsNullOrEmpty(viewModel.grupo) &&
                !string.IsNullOrEmpty(viewModel.tienda) &&
                !string.IsNullOrEmpty(viewModel.zona))
            {
                viewModel.conteo.Clear();
                //Cargar fichero si existe y permitir la entrada
                string fileName = Path.Combine(FileSystem.AppDataDirectory, App.GnxSettings.ficheroconteo + viewModel.grupo + ".txt");
                if (File.Exists(fileName))
                {
                    var lines = File.ReadLines(fileName);
                    foreach (var linea in lines)
                    {
                        if (string.IsNullOrEmpty(linea)) continue;
                        var cont = new conteo();
                        cont.setGnxMsg(linea);
                        viewModel.conteo.Insert(0, cont);
                    }
                }
                newPlu.IsEnabled = true;
                newPlu.Focus();
            }
         }

        async void Tienda_Completed(System.Object sender, System.EventArgs e)
        {
            if(Tienda.Text.Length > 2)
            {
                await DisplayAlert(AppResources.Error, AppResources.ErrorTienda, AppResources.Aceptar);
                Tienda.Text = "";
                Tienda.Focus();
                return;
            }
            Zona.Focus();
        }

        void Grupo1_CheckedChanged(System.Object sender, Xamarin.Forms.CheckedChangedEventArgs e)
        {
            Tienda.Focus();
        }


        async void Cantidad_Completed(System.Object sender, System.EventArgs e)
        {
            double cant;
            Cantidad.Text = Cantidad.Text.Replace('.', ',');
            if (Cantidad.Text.Length < 6 && double.TryParse(Cantidad.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out cant))
            {
                Total.Text = (double.Parse(viewModel.arti.unidpaquete) * cant).ToString("####.000", CultureInfo.CurrentCulture);
                conteo cont = new conteo();
                cont.tienda = Tienda.Text;
                cont.zona = Zona.Text;
                cont.descripcion = viewModel.arti.descripcion.Trim();
                cont.interno = viewModel.arti.interno.Trim();
                cont.cantidad = double.Parse(Total.Text, CultureInfo.CurrentCulture);
                viewModel.conteo.Insert(0, cont);
                viewModel.arti = new Arti();
                newPlu.Text = "";
                Cantidad.Text = "";
                Total.Text = "";
                Cantidad.IsEnabled = false;
                cont.GuardarFichero(viewModel.grupo);
                newPlu.Focus();

            }
            else
            {
                await DisplayAlert(AppResources.Error, AppResources.ErrorCantidad,AppResources.Aceptar);
                Cantidad.Text = "";
                Cantidad.Focus();
            }
        }

        void Cantidad_Focused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            Cantidad.CursorPosition = 0;
            Cantidad.SelectionLength = Cantidad.Text.Length;

        }
    }
}

