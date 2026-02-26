using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Windows.Input;
using WARBIMPRO.Models;
using WARBIMPRO.Services;

namespace WARBIMPRO.ViewModels
{
    public class DuplicateElementViewModel : ObservableObject
    {
        private readonly RevitTypeService _typeService;
        private readonly ElementType _originalType;

        public string NewTypeName { get; set; }
        public ObservableCollection<TypeParameterModel> Parameters { get; set; }

        public ICommand DuplicateCommand { get; }

        public DuplicateElementViewModel(Document doc, Element selectedElement)
        {
            _typeService = new RevitTypeService(doc);

            _originalType = _typeService.GetElementType(selectedElement);

            NewTypeName = _originalType.Name + "_Copia";

            Parameters = new ObservableCollection<TypeParameterModel>(
                LoadEditableParameters(_originalType)
            );

            DuplicateCommand = new RelayCommand(DuplicateType);
        }

        private void DuplicateType()
        {
            var newType = _typeService.DuplicateType(_originalType, NewTypeName);
            _typeService.UpdateParameters(newType, Parameters.ToList());
        }

        private List<TypeParameterModel> LoadEditableParameters(ElementType type)
        {
            var list = new List<TypeParameterModel>();

            foreach (Parameter param in type.Parameters)
            {
                if (param == null) continue;
                if (param.IsReadOnly) continue;
                if (!param.HasValue) continue;

                var model = new TypeParameterModel
                {
                    Name = param.Definition.Name,
                    StorageType = param.StorageType,
                    IsEditable = true
                };

                switch (param.StorageType)
                {
                    case StorageType.Double:
                        model.Value = param.AsDouble();
                        break;

                    case StorageType.Integer:
                        model.Value = param.AsInteger();
                        break;

                    case StorageType.String:
                        model.Value = param.AsString();
                        break;

                    case StorageType.ElementId:
                        continue;
                }

                list.Add(model);
            }

            return list;
        }
    }
}
