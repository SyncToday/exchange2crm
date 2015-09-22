using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace exchange2crm.Interfaces
{
    public interface IImportAllContacts : IGrainWithIntegerKey
    {
        Task<IEnumerable<IContact>> ImportAll(IEnumerable<IContact> contacts);
    }
}
