using GenesixInv.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GenesixInv.Services
{
    class RestService
    {

        HttpClient _client;
        string ApiUrl = "api/v1/gnx-inv/";

        public RestService()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<RestRespuesta> TestAsync(RestBase send)
        {
            RestRespuesta resp = new RestRespuesta();
            try
            {
                Uri uri = new Uri(App.GnxSettings.url + ApiUrl + "test");
                string json = JsonConvert.SerializeObject(send);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                _client.Timeout = TimeSpan.FromSeconds(10);
                response = await _client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    var respcon = await response.Content.ReadAsStringAsync();
                    resp = JsonConvert.DeserializeObject<RestRespuesta>(respcon);

                }

            }
            catch (Exception ex)
            {
                resp.status = "Error";
                resp.error = ex.Message;

            }
            return resp;
        }

        public async Task<RestRespuesta> DescargarAsync(RestRespuesta send)
        {
            RestRespuesta resp = new RestRespuesta();
            try
            {
                Uri uri = new Uri(App.GnxSettings.url + ApiUrl + "descargar");
                string json = JsonConvert.SerializeObject(send);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = await _client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    var respcon = await response.Content.ReadAsStringAsync();
                    resp = JsonConvert.DeserializeObject<RestRespuesta>(respcon);

                }
                else
                {
                    resp.status = "Error";
                    resp.error = await response.Content.ReadAsStringAsync();
                }

            }
            catch (Exception ex)
            {
                resp.status = "Error";
                resp.error = ex.Message;

            }
            return resp;
        }
    }
}
