namespace exchange2crm

open exchange2crm.Interfaces

type SyncedContact = 
    {
        FirstName   : string;
        LastName    : string;
        Company     : string;
        JobTitle    : string;
        Email       : string;
        PhoneMobile : string;
        PhoneWork   : string;
        Notes       : string;
        UniqueId    : string;
    }
    interface IContact with
        member x.FirstName   = x.FirstName
        member x.LastName    = x.LastName
        member x.Company     = x.Company
        member x.JobTitle    = x.JobTitle
        member x.Email       = x.Email
        member x.PhoneMobile = x.PhoneMobile
        member x.PhoneWork   = x.PhoneWork
        member x.Notes       = x.Notes
        member x.UniqueId    = x.UniqueId