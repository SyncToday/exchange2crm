using System.Threading.Tasks;
using Orleans;
using System;

namespace exchange2crm.Interfaces
{
    public interface ISaveSingleContact : IGrainWithGuidKey
	{
		Task Import(IContact contact);
	}

}
