using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaAC
{
    public enum Status
    {
        None,
        NoConnetion,
        NoResult,
        ValidResult,
        NeedValidation,
        NoValidated,
        Validated,
        ValidatedByUser,
        UnValidatedByUser,
        CancelledByUser
    }

    
}
