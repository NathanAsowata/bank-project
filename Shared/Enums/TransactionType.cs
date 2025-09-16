using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums
{
    public enum TransactionType { 
        Deposit, 
        Withdrawal, 
        InternalTransfer, 
        ExternalTransfer, 
        Fee, 
        Interest, 
        Pending, 
        Flagged } // Added pending/flagged for manager flow (technical merit)
}
