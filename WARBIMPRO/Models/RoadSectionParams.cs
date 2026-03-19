using Autodesk.Revit.DB;

namespace WARBIMPRO.Models
{
    public class RoadSectionParams
    {
        // ── Vía ─────────────────────────────────────────────────────────────
        public double RoadWidthMeters { get; set; } = 6.0;
        public double CrossSlopePercent { get; set; } = 2.0;
        public double StartElevationMeters { get; set; } = 0.0;
        public double LongSlopePercent { get; set; } = 5.0;
        public double StationSpacingMeters { get; set; } = 5.0;

        // ── Andén izquierdo ──────────────────────────────────────────────────
        public bool HasLeftSidewalk { get; set; } = true;
        public double LeftSidewalkWidthMeters { get; set; } = 1.5;

        // ── Andén derecho ────────────────────────────────────────────────────
        public bool HasRightSidewalk { get; set; } = true;
        public double RightSidewalkWidthMeters { get; set; } = 1.5;

        // ── Pendiente andenes ────────────────────────────────────────────────
        public double SidewalkSlopePercent { get; set; } = 3.0;

        // ── Materiales ───────────────────────────────────────────────────────
        public ElementId RoadMaterialId { get; set; } = ElementId.InvalidElementId;
        public ElementId LeftMaterialId { get; set; } = ElementId.InvalidElementId;
        public ElementId RightMaterialId { get; set; } = ElementId.InvalidElementId;
    }
}
