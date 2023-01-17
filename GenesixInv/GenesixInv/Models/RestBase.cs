using System;
using System.Collections.Generic;
using System.Text;
using GenesixInv.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GenesixInv.Models
{
    [Serializable]
    public class RestBase
    {
        public string terminal { get; set; }
        public string token { get; set; }
        public string status { get; set; }
        public string error { get; set; }
        public string buildnumber
        {
            get
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    return "I" + AppInfo.BuildString;
                }
                else
                if (Device.RuntimePlatform == Device.Android)
                {
                    return "A" + AppInfo.BuildString;
                }
                return "";
            }
        }
        public string buildkey { get; set; }
        public string iddevice
        {
            get
            {
                return DependencyService.Get<IDeviceService>().getId();
            }
        }
        public string dispositivo
        {
            get
            {
                return DeviceInfo.Name;
            }
        }


    }
}
