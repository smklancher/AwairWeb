using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AwairApi
{
    public  class AwairServiceOptions
    {
        public HttpClient Client { get; set; }
        public bool UseFahrenheit { get; set; }= true;
        public string BearerToken { get; set; }= string.Empty;
        public string Proxy { get; set; }=string.Empty;
    }
}
