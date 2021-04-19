using System;
using System.Collections.Generic;
using System.Web.Http;
using WcfService.Clients;

namespace BridgeAPI.Controllers
{
    public class TestController:ApiController
    {
        public class GetDataDto 
        {
            public int Value { get; set; }
        }

        [HttpPost]
        public Dictionary<string, object> GetData([FromBody] GetDataDto request) 
        {
            ServiceClient proxy = new WcfService.Clients.ServiceClient();
            Guid productID = Guid.NewGuid();
            var respose = proxy.GetData(1);
            // https://www.codemag.com/article/0809101/WCF-the-Manual-Way%E2%80%A6-the-Right-Way
            return new Dictionary<string, object> {                
                { "Result", respose}
            };
        }

        [HttpPost]
        public string GetData([FromBody]string input)
        {
            return $"hello {input}";
        }
    }
}