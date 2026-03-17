namespace WARBIMPRO.Models
{
    public class RoadSectionParams
    {
        /// <summary>Ancho total de la vía en metros.</summary>
        public double RoadWidthMeters { get; set; } = 6.0;

        /// <summary>Pendiente transversal en % — baja del centro al borde.</summary>
        public double CrossSlopePercent { get; set; } = 2.0;

        /// <summary>Pendiente longitudinal en % — a lo largo del eje.</summary>
        public double LongSlopePercent { get; set; } = 5.0;

        /// <summary>Cota Z del inicio del eje en metros.</summary>
        public double StartElevationMeters { get; set; } = 0.0;

        /// <summary>Espaciado entre estaciones en metros.</summary>
        public double StationSpacingMeters { get; set; } = 5.0;
    }
}