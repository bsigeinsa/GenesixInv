using GenesixInv.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenesixInv.Models
{
    [Serializable]
    public class RestRespuesta : RestBase
    {
        public string file { get; set; }
        public string fileContent { get; set; }
        public string fileHash { get; set; }
        public string filedate { get; set; }

    }
}
