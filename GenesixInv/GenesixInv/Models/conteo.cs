using System;
using System.Globalization;
using System.IO;
using System.Text;
using Xamarin.Essentials;

namespace GenesixInv.Models
{
    public class conteo
    {
        public string tienda { get; set; }
        public string zona { get; set; }
        public string descripcion { get; set; }
        public string interno { get; set; }
        public double cantidad { get; set; }

        public conteo()
        {

        }
         public string getGnxMsg()
        {
            string res;
            res = tienda + "\t";
            res+= zona + "\t";
            res += descripcion + "\t";
            res += interno + "\t";
            res += cantidad.ToString("#####.000", CultureInfo.CurrentCulture)+"\r";
            return res;
        }
        public void setGnxMsg(string msg)
        {
            var campos = msg.Split('\t');
            tienda = campos[0];
            zona = campos[1];
            descripcion = campos[2];
            interno = campos[3];
            cantidad = double.Parse(campos[4], CultureInfo.CurrentCulture);
        }

        public void GuardarFichero(string grupo)
        {
            string filename = Path.Combine(FileSystem.AppDataDirectory, App.GnxSettings.ficheroconteo + grupo + ".txt");
            using (StreamWriter sw = new StreamWriter(new FileStream(filename,FileMode.Append,FileAccess.Write)))
            {
                sw.WriteLine(getGnxMsg());

            }

        }
    }
}

