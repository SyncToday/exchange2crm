using Orleans;
using System;

namespace exchange2crm.Interfaces
{
    public interface IContact
    {
        String FirstName { get; }
        String LastName { get; }
        String Company { get; }
        String JobTitle { get; }
        String Email { get; }
        String PhoneMobile { get; }
        String PhoneWork { get; }
        String Notes { get; }
        String UniqueId { get; }
    }
}
