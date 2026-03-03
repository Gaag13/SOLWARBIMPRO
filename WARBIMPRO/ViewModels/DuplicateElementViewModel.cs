using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Windows.Input;
using WARBIMPRO.Models;
using WARBIMPRO.Services;

namespace WARBIMPRO.ViewModels
{
    public class DuplicateElementViewModel : ObservableObject
    {
        private readonly RevitTypeService _typeService;
        private readonly ElementType _originalType;
        private readonly Element _originalElement;
       

        public string NewTypeName { get; set; }
        public string OriginalTypeName { get; set; }    
        public string CategoryName { get; }
        public bool IsWallOrFloor { get; set; }
        public bool IsFramingOrColumn { get; set; }
        public bool IsFoundation { get; set; }


        public ICommand DuplicateCommand { get; }

        public DuplicateElementViewModel(Document doc, Element selectedElement)
        {
            _typeService = new RevitTypeService(doc);           
           
            _originalElement = selectedElement;
            _originalType = _typeService.GetElementType(selectedElement);

            OriginalTypeName = _originalType.Name;

            NewTypeName = _originalType.Name + "_Copia";
            
            CategoryName= selectedElement.Category?.Name ?? "Sin categoría";

            var bic = (BuiltInCategory)selectedElement.Category?.Id.Value;

            IsFramingOrColumn =
                bic == BuiltInCategory.OST_StructuralColumns ||
                bic == BuiltInCategory.OST_StructuralFraming;
                
            IsWallOrFloor =
                 bic == BuiltInCategory.OST_Walls ||
                 bic == BuiltInCategory.OST_Floors;

            IsFoundation= bic == BuiltInCategory.OST_StructuralFoundation;


            DuplicateCommand = new RelayCommand(DuplicateType);
        }

        private void DuplicateType()
        {
            if (string.IsNullOrWhiteSpace(NewTypeName))
                return;

            var newType = _typeService.DuplicateType(_originalType, NewTypeName);

            if(newType == null) 
                return;

            _typeService.UpdateTypeDimensions(newType, ParamValue1, ParamValue2,ParamValue3);

            _typeService.AssignTypeToElement(_originalElement, newType);
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
        private double _paramValue3;
        public double ParamValue3
        {
            get => _paramValue3;
            set => SetProperty(ref _paramValue3, value);
        }
    }
}
