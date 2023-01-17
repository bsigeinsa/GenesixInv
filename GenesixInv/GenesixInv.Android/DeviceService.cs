using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenesixInv.Droid;
using GenesixInv.Services;
using static Android.Provider.Settings;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceService))]

namespace GenesixInv.Droid
{
    public class DeviceService : IDeviceService
    {
        public string getId()
        {
            string id = "";
            try
            {
                id = Build.GetSerial();
            }
            catch (Exception)
            {
            }
            if (string.IsNullOrWhiteSpace(id) || id == Build.Unknown || id == "0")
            {
                try
                {
                    var context = Android.App.Application.Context;
                    id = Secure.GetString(context.ContentResolver, Secure.AndroidId);
                }
                catch (Exception)
                {

                }
            }

            return id;
        }
        public bool getTracking()
        {
            return true;
        }
    }
}