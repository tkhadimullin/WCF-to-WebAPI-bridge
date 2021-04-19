using System;
using System.Threading.Tasks;

namespace WcfClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var client = new ServiceReference.ServiceClient();
            var response = await client.GetDataAsync(1);
            return;
        }
    }
}
