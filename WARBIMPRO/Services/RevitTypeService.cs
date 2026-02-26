using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WARBIMPRO.Models;
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
            using (Transaction t = new Transaction(_doc, "Duplicar Tipo"))
            {
                t.Start();
                ElementType newType = type.Duplicate(newName) as ElementType;
                t.Commit();
                return newType;
            }
        }

        public void UpdateParameters(ElementType type, List<TypeParameterModel> parameters)
        {
            using (Transaction t = new Transaction(_doc, "Modificar Parámetros"))
            {
                t.Start();

                foreach (var paramModel in parameters)
                {
                    Parameter p = type.LookupParameter(paramModel.Name);
                    if (p == null || p.IsReadOnly) continue;

                    switch (paramModel.StorageType)
                    {
                        case StorageType.Double:
                            p.Set((double)paramModel.Value);
                            break;

                        case StorageType.Integer:
                            p.Set((int)paramModel.Value);
                            break;

                        case StorageType.String:
                            p.Set((string)paramModel.Value);
                            break;
                    }
                }

                t.Commit();
            }
        }
    }
}
