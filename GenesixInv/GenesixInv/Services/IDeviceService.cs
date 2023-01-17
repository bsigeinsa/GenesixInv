using System;
using System.Collections.Generic;
using System.Text;

namespace GenesixInv.Services
{
    public interface IDeviceService
    {
        string getId();
        bool getTracking();
    }
}
