namespace WARBIMPRO.Models
{
    /// <summary>
    /// Parámetros de la sección tipo de vía.
    /// Por ahora solo vía — andenes se agregan en siguiente fase.
    /// </summary>
    public class RoadSectionParams
    {
        /// <summary>Ancho total de la vía en metros.</summary>
        public double RoadWidthMeters { get; set; } = 6.0;

        /// <summary>
        /// Pendiente transversal de la vía en porcentaje.
        /// Ej: 2.0 = 2% — la vía baja del centro hacia los bordes.
        /// </summary>
        public double CrossSlopePercent { get; set; } = 2.0;

        /// <summary>
        /// Espaciado entre estaciones transversales en metros.
        /// Cada N metros el addin calcula una fila de puntos.
        /// </summary>
        public double StationSpacingMeters { get; set; } = 5.0;
    }
}