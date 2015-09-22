using System.Threading.Tasks;
using Orleans;
using System;

namespace exchange2crm.Interfaces
{
    public interface IImportSingleContact : IGrainWithIntegerKey
	{
		Task<IContact> Import(IContact contact);
	}

}
