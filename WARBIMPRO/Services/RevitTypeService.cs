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
            var collector = new FilteredElementCollector(_doc)
                .OfClass(typeof(ElementType))
                .Cast<ElementType>()
                .FirstOrDefault(t => t.Name.Equals(newName));

            if (collector != null)
                throw new InvalidOperationException("Ya existe un tipo con ese nombre.");

            using (Transaction t = new Transaction(_doc, "Duplicar Tipo Estructural"))
            {
                t.Start();

                ElementType newType = type.Duplicate(newName) as ElementType;

                t.Commit();

                return newType;
            }
        }
        public void UpdateTypeDimensions(ElementType type, double value1, double value2)
        {
            using (Transaction t = new Transaction(_doc, "Modificar Dimensiones"))
            {
                t.Start();

                double v1 = Utils.Tools.Cm_to_Feet(value1);
                double v2 = Utils.Tools.Cm_to_Feet(value2);

                BuiltInCategory bic = (BuiltInCategory)type.Category.Id.Value;

                if (bic == BuiltInCategory.OST_Walls)
                {
                    // Espesor de muro
                    Parameter width = type.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM);
                    if (width != null && !width.IsReadOnly)
                        width.Set(v1);
                }

                else if (bic == BuiltInCategory.OST_Floors)
                {
                    // Espesor de piso
                    Parameter thickness = type.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
                    if (thickness != null && !thickness.IsReadOnly)
                        thickness.Set(v1);
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

                t.Commit();
            }
        }

    }
}
