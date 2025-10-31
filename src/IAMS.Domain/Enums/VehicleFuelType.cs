using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum VehicleFuelType
    {
        Gasoline = 1,         // Benzin
        Diesel = 2,           // Dizel
        LPG = 3,              // LPG
        Electric = 4,         // Elektrik
        Hybrid = 5,           // Hibrit
        GasolineLPG = 6,      // Benzin+LPG
        HybridGasoline = 7,   // Hibrit Benzin
        HybridDiesel = 8      // Hibrit Dizel
    }
}
