﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stealth.Plugins.ALPRPlus.API.Types.Enums
{
    /// <summary>
    /// The type of the pre-defined alert.
    /// </summary>
    public enum EAlertType
    {
        Null,
        Stolen_Vehicle,
        Owner_Wanted,
        Unregistered_Vehicle,
        Registration_Expired,
        No_Insurance,
        Insurance_Expired,
        UnTaxed,
        DrugsMarker

    }

    public enum ECamera
    {
        Null,
        Driver_Front,
        Driver_Rear,
        Passenger_Front,
        Passenger_Rear
    }
}
