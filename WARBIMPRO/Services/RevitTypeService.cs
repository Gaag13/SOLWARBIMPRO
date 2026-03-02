using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WARBIMPRO.Models;
using WARBIMPRO.Utils;
using static WARBIMPRO.Models.TypeParameterModel;

namespace WARBIMPRO.Services
{
    public class RevitTypeService
    {
        private readonly Document _doc;

        public RevitTypeService(Document doc)
        {
            _doc = doc;
        }

        public ElementType GetElementType(Element element)
        {
            return _doc.GetElement(element.GetTypeId()) as ElementType;
        }
        

        public ElementType DuplicateType(ElementType type, string newName)
        {
            // Verificar si ya existe un tipo con ese nombre
            

            try
            {
                var collector = new FilteredElementCollector(_doc)
                .OfClass(typeof(ElementType))
                .Cast<ElementType>()
                .FirstOrDefault(t => t.Name.Equals(newName));

               
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                TaskDialog.Show("WARBIMPRO", $"Ya existe un tipo con el nombre '{newName}'. Elige otro nombre.");

            }



            using (Transaction t = new Transaction(_doc, "Duplicar Tipo Estructural"))
            {
                t.Start();

                ElementType newType = type.Duplicate(newName) as ElementType;                
                t.Commit();

                return newType;
            }
        }

        public void AssignTypeToElement(Element element, ElementType newType)
        {
            using (Transaction t = new Transaction(_doc, "Asignar Nuevo Tipo"))
            {
                t.Start();

                element.ChangeTypeId(newType.Id);

                t.Commit();
            }
        }
        public void UpdateTypeDimensions(ElementType type, double value1, double value2,double value3)
        {
            using (Transaction t = new Transaction(_doc, "Modificar Dimensiones"))
            {
                t.Start();

                double v1 = Utils.Tools.Cm_to_Feet(value1);
                double v2 = Utils.Tools.Cm_to_Feet(value2);
                double v3 = Utils.Tools.Cm_to_Feet(value3); 

                BuiltInCategory bic = (BuiltInCategory)type.Category.Id.Value;

                if (bic == BuiltInCategory.OST_Walls)

                {
                    if (type is WallType wallType)
                    {
                        CompoundStructure cs = wallType.GetCompoundStructure();

                        if (cs != null)
                        {
                            double currentTotal = cs.GetWidth();

                            if (currentTotal > 0)
                            {
                                double factor = v1 / currentTotal;

                                for (int i = 0; i < cs.LayerCount; i++)
                                {
                                    double layerWidth = cs.GetLayerWidth(i);
                                    cs.SetLayerWidth(i, layerWidth * factor);
                                }

                                wallType.SetCompoundStructure(cs);
                            }
                        }
                    }
                }

                else if (bic == BuiltInCategory.OST_Floors)
                {
                    if (type is FloorType floorType)
                    {
                        CompoundStructure cs = floorType.GetCompoundStructure();

                        if (cs != null)
                        {
                            double currentTotal = cs.GetWidth();

                            if (currentTotal > 0)
                            {
                                double factor = v1 / currentTotal;

                                for (int i = 0; i < cs.LayerCount; i++)
                                {
                                    double layerWidth = cs.GetLayerWidth(i);
                                    cs.SetLayerWidth(i, layerWidth * factor);
                                }

                                floorType.SetCompoundStructure(cs);
                            }
                        }
                    }
                }

                else if (bic == BuiltInCategory.OST_StructuralFraming)
                {
                    // Vigas suelen tener parámetros b y h
                    Parameter b = type.LookupParameter("b");
                    Parameter h = type.LookupParameter("h");

                    if (b != null && !b.IsReadOnly)
                        b.Set(v1);

                    if (h != null && !h.IsReadOnly)
                        h.Set(v2);
                }

                else if (bic == BuiltInCategory.OST_StructuralColumns)
                {
                    Parameter b = type.LookupParameter("b");
                    Parameter h = type.LookupParameter("h");

                    if (b != null && !b.IsReadOnly)
                        b.Set(v1);

                    if (h != null && !h.IsReadOnly)
                        h.Set(v2);
                }

                else if (bic == BuiltInCategory.OST_StructuralFoundation)
                {
                    Parameter width= type.LookupParameter("Width");
                    Parameter length = type.LookupParameter("Length");
                    Parameter e= type.LookupParameter("Foundation Thickness");

                    if (width != null && !width.IsReadOnly)
                        width.Set(v1);
                    if (length != null && !length.IsReadOnly)
                        length.Set(v2);
                    if (e != null && !e.IsReadOnly)
                        e.Set(v3);
                }

                    t.Commit();
            }
        }

    }
}
