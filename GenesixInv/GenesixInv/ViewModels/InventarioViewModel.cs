using System;
using System.Collections.ObjectModel;
using GenesixInv.Models;

namespace GenesixInv.ViewModels
{
	public class InventarioViewModel : BaseViewModel
	{
        string _grupo = "1";
        public string grupo
        {
            get { return _grupo; }
            set { SetProperty(ref _grupo, value); }
        }
        string _tienda = string.Empty;
        public string tienda
        {
            get { return _tienda; }
            set { SetProperty(ref _tienda, value); }
        }
        string _zona = string.Empty;
        public string zona
        {
            get { return _zona; }
            set { SetProperty(ref _zona, value); }
        }
        string _ean = string.Empty;
        public string ean
        {
            get { return _ean; }
            set { SetProperty(ref _ean, value); }
        }
        string _cantidad = string.Empty;
        public string cantidad
        {
            get { return _cantidad; }
            set { SetProperty(ref _cantidad, value); }
        }
        string _total = string.Empty;
        public string total
        {
            get { return _total; }
            set { SetProperty(ref _total, value); }
        }
        Arti _arti = new Arti();
        public Arti arti
        {
            get { return _arti; }
            set { SetProperty(ref _arti, value); }
        }
        public ObservableCollection<conteo> conteo { get; set; }
        
  
        public InventarioViewModel()
		{
            conteo = new ObservableCollection<conteo>();
            arti = new Arti();
		}
	}
}

