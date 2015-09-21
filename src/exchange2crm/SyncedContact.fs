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
    }
    interface IContact with
        member x.Mark() = 0 |> ignore
