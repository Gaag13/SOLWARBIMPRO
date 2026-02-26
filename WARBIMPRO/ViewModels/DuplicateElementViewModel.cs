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
        public string CategoryName { get; }
        public bool IsStructural { get; }
        public bool IsWall { get; }
        public bool IsFloor { get; }


        public ICommand DuplicateCommand { get; }

        public DuplicateElementViewModel(Document doc, Element selectedElement)
        {
            _typeService = new RevitTypeService(doc);

            _originalType = _typeService.GetElementType(selectedElement);

            NewTypeName = _originalType.Name + "_Copia";
            
            CategoryName= selectedElement.Category?.Name ?? "Sin categoría";

            var bic = (BuiltInCategory)selectedElement.Category?.Id.Value;

            IsStructural =
                bic == BuiltInCategory.OST_StructuralColumns ||
                bic == BuiltInCategory.OST_StructuralFraming ||
                bic == BuiltInCategory.OST_StructuralFoundation;

            IsWall = bic == BuiltInCategory.OST_Walls;
            IsFloor = bic == BuiltInCategory.OST_Floors;

            DuplicateCommand = new RelayCommand(DuplicateType);
        }

        private void DuplicateType()
        {
            if (string.IsNullOrWhiteSpace(NewTypeName))
                return;

            var newType = _typeService.DuplicateType(_originalType, NewTypeName);

            _typeService.UpdateTypeDimensions(newType, ParamValue1, ParamValue2);
        }
        private double _paramValue1;
        public double ParamValue1
        {
            get => _paramValue1;
            set => SetProperty(ref _paramValue1, value);
        }

        private double _paramValue2;
        public double ParamValue2
        {
            get => _paramValue2;
            set => SetProperty(ref _paramValue2, value);
        }

    }
}
