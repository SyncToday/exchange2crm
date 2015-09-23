using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exchange2crm.Interfaces
{
    public interface IImportExchangeContacts : IGrainWithIntegerKey
    {
        Task Import();
    }
}
