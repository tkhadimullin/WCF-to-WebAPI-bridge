using System;
using System.ServiceModel;

namespace WcfService.Clients
{
    public class ServiceClient : ClientBase<IService>, IService
    {
        public string GetData(int value)
        {
            return Channel.GetData(value);
        }

        public string GetDataOut(int value, out DateTime creationDate)
        {
            return Channel.GetDataOut(value, out creationDate);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            return Channel.GetDataUsingDataContract(composite);
        }
    }
}