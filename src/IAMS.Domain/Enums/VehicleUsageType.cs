using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum VehicleUsageType
    {
        Private = 1,          // Hususi - Özel Kullanım
        Commercial = 2,       // Ticari
        Taxi = 3,             // Taksi
        RentalCar = 4,        // Kiralık Araç
        PublicTransport = 5,  // Toplu Taşıma
        Ambulance = 6,        // Ambulans
        Agriculture = 7,      // Tarım
        Construction = 8,     // İnşaat
        Courier = 9,          // Kurye
        SchoolBus = 10        // Okul Servisi
    }
}
