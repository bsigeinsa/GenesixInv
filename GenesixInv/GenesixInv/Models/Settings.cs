using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace GenesixInv.Models
{
    public class Settings
    {
        public Settings()
        {
            surl = Preferences.Get("url", "");
            surlarticulos = Preferences.Get("urlarticulos", "");
            sficheroconteo = Preferences.Get("ficheroconteo", "conteo");
            smascara = Preferences.Get("mascara", "");
            smascara1 = Preferences.Get("mascara1", "");
            smascara2 = Preferences.Get("mascara2", "");


        }
        string surl;
        public string url
        {
            get
            {
                return surl;
            }
            set
            {
                surl = value;
                Preferences.Set("url", value);
            }
        }
        string surlarticulos;
        public string urlarticulos
        {
            get
            {
                return surlarticulos;
            }
            set
            {
                surlarticulos = value;
                Preferences.Set("urlarticulos", value);
            }
        }
        string sficheroconteo;
        public string ficheroconteo
        {
            get { return sficheroconteo; }
            set
            {
                sficheroconteo = value;
                Preferences.Set("ficheroconteo", value);
            }
        }

        string smascara;
        public string mascara
        {
            get { return smascara; }
            set
            {
                smascara = value;
                Preferences.Set("mascara", value);
            }
        }
        string smascara1;
        public string mascara1
        {
            get { return smascara1; }
            set
            {
                smascara1 = value;
                Preferences.Set("mascara1", value);
            }
        }
        string smascara2;
        public string mascara2
        {
            get { return smascara2; }
            set
            {
                smascara2 = value;
                Preferences.Set("mascara2", value);
            }
        }
    }
}
