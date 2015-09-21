using System.Threading.Tasks;
using Orleans;

namespace exchange2crm.Interfaces
{
    public interface IContact
    {
        void Mark();
    }

    /// <summary>
    /// Grain interface IGrain1
    /// </summary>
	public interface IImportExchangeContactToCRM : IGrain
    {
        Task<IContact> import(IContact contact);
    }

}
