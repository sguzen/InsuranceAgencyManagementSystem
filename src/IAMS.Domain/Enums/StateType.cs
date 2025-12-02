namespace IAMS.Domain.Enums
{
    /// <summary>
    /// Zeyil Tipi - Endorsement/Policy State Type
    /// Represents the type of endorsement or the main policy state
    /// </summary>
    public enum StateType
    {
        /// <summary>
        /// Main/Original Policy (Ana Poliçe) - Default for 000
        /// </summary>
        MainPolicy = 0,

        /// <summary>
        /// Premium Increase (Prim Artışı)
        /// </summary>
        PremiumIncrease = 1,

        /// <summary>
        /// Premium Decrease (Prim Azalışı)
        /// </summary>
        PremiumDecrease = 2,

        /// <summary>
        /// Coverage Addition (Teminat Ekleme)
        /// </summary>
        CoverageAddition = 3,

        /// <summary>
        /// Coverage Removal (Teminat Çıkarma)
        /// </summary>
        CoverageRemoval = 4,

        /// <summary>
        /// Coverage Change (Teminat Değişikliği)
        /// </summary>
        CoverageChange = 5,

        /// <summary>
        /// Vehicle Change (Araç Değişikliği)
        /// </summary>
        VehicleChange = 6,

        /// <summary>
        /// Customer Change (Müşteri Değişikliği)
        /// </summary>
        CustomerChange = 7,

        /// <summary>
        /// Term Extension (Vade Uzatımı)
        /// </summary>
        TermExtension = 8,

        /// <summary>
        /// Term Reduction (Vade Azaltımı)
        /// </summary>
        TermReduction = 9,

        /// <summary>
        /// Cancellation (İptal)
        /// </summary>
        Cancellation = 10,

        /// <summary>
        /// Correction (Düzeltme)
        /// </summary>
        Correction = 11,

        /// <summary>
        /// Renewal (Yenileme)
        /// </summary>
        Renewal = 12,

        /// <summary>
        /// Other (Diğer)
        /// </summary>
        Other = 99
    }
}
