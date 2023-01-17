using GenesixInv.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenesixInv.Models
{
    [Serializable]
    public class RestRespuesta : RestBase
    {
        public string file { get; set; }
        public string fileContent { get; set; }

    }
}
