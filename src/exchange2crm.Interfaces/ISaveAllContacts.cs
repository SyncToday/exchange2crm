using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace exchange2crm.Interfaces
{
    public interface ISaveAllContacts : IGrainWithIntegerKey
    {
        Task ImportAll(IEnumerable<IContact> contacts);
    }
}
