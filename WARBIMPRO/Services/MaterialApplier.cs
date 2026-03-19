using Autodesk.Revit.DB;
using System;

namespace WARBIMPRO.Services
{
    /// <summary>
    /// Aplica materiales a subdivisiones de Toposolid.
    /// Se llama después de CreateSubDivision para asignar el material correcto
    /// a cada franja (vía, andén izquierdo, andén derecho).
    /// </summary>
    public static class MaterialApplier
    {
        /// <summary>
        /// Asigna un material a una subdivisión de Toposolid.
        /// La subdivisión expone el material a través del parámetro BuiltInParameter.MATERIAL_ID_PARAM
        /// o mediante su propiedad de material de la capa estructural.
        /// </summary>
        public static void ApplyMaterial(Document doc, Element subdivision, ElementId materialId)
        {
            if (subdivision == null) return;
            if (materialId == null || materialId == ElementId.InvalidElementId) return;

            try
            {
                // Intentar por parámetro directo de material
                var matParam = subdivision.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                if (matParam != null && !matParam.IsReadOnly)
                {
                    matParam.Set(materialId);
                    return;
                }

                // Alternativa: buscar cualquier parámetro de tipo material
                foreach (Parameter param in subdivision.Parameters)
                {
                    if (param.StorageType == StorageType.ElementId
                        && !param.IsReadOnly
                        && param.Definition.Name.ToLower().Contains("material"))
                    {
                        param.Set(materialId);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // No interrumpir el flujo si el material falla — la geometría ya está creada
                System.Diagnostics.Debug.WriteLine($"MaterialApplier: {ex.Message}");
            }
        }
    }
}
