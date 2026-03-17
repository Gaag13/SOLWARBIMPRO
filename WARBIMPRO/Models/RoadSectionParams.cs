namespace WARBIMPRO.Models
{
    public class RoadSectionParams
    {
        // ── Vía ──────────────────────────────────────────────────────────────
        public double RoadWidthMeters { get; set; } = 6.0;
        public double CrossSlopePercent { get; set; } = 2.0;
        public double LongSlopePercent { get; set; } = 5.0;
        public double StartElevationMeters { get; set; } = 0.0;
        public double StationSpacingMeters { get; set; } = 5.0;

        // ── Andenes ──────────────────────────────────────────────────────────
        public bool HasLeftSidewalk { get; set; } = true;
        public double LeftSidewalkWidthMeters { get; set; } = 1.5;

        public bool HasRightSidewalk { get; set; } = true;
        public double RightSidewalkWidthMeters { get; set; } = 1.5;

        /// <summary>
        /// Pendiente del andén hacia afuera de la vía (%).
        /// Normalmente 2-3% para drenaje.
        /// </summary>
        public double SidewalkSlopePercent { get; set; } = 3.0;
    }
}