using System;
using System.Text;

namespace GenesixInv.Models
{
    [Serializable]
    public class Arti
    {
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public string dept { get; set; }
        public string categoria { get; set; }
        public double precio { get; set; }
        public double pvpc1 { get; set; }
        public double pvpc2 { get; set; }
        public double pvpc3 { get; set; }
        public double pvpc4 { get; set; }
        public double pvpofe { get; set; }
        public string codimp1 { get; set; }
        public string codimp2 { get; set; }
        public string codimp3 { get; set; }
        public string codimp4 { get; set; }
        public string codimp5 { get; set; }
        public string descuento { get; set; }
        public string artneg { get; set; }
        public string verifprecios { get; set; }
        public string pesable { get; set; }
        public string prepmerc { get; set; }
        public string tipopaquete { get; set; }
        public string unidpaquete { get; set; }
        public string pluenl { get; set; }
        public string codlot { get; set; }
        public double puntos { get; set; }
        public double puntosofe { get; set; }
        public string interno { get; set; }
        public string familia { get; set; }
        public string subfam { get; set; }
        public string dummy { get; set; }

         public void setGnxMsg(string msg)
        {
            var campos = msg.Split('\t');
            codigo = campos[0];
            descripcion = campos[1];
            dept = campos[2];
            categoria = campos[3];
            double aux;
            precio = double.TryParse(campos[4], out aux) ? aux : 0;
            pvpc1 = double.TryParse(campos[5], out aux) ? aux : 0;
            pvpc2 = double.TryParse(campos[6], out aux) ? aux : 0;
            pvpc3 = double.TryParse(campos[7], out aux) ? aux : 0;
            pvpc4 = double.TryParse(campos[8], out aux) ? aux : 0;
            pvpofe = double.TryParse(campos[9], out aux) ? aux : 0;
            codimp1 = campos[10];
            codimp2 = campos[11];
            codimp3 = campos[12];
            codimp4 = campos[13];
            codimp5 = campos[14];
            descuento = campos[15];
            artneg = campos[16];
            verifprecios = campos[17];
            pesable = campos[18];
            prepmerc = campos[19];
            tipopaquete = campos[20];
            unidpaquete = campos[21];
            pluenl = campos[22];
            codlot = campos[23];
            puntos = double.TryParse(campos[24], out aux) ? aux : 0;
            puntosofe = double.TryParse(campos[25], out aux) ? aux : 0;
            interno = campos[26];
            familia = campos[27];
            subfam = campos[28];
            dummy = campos[29];
        }
    }
}

