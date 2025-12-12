using Autodesk.Revit.DB;
using System;

namespace WARBIMPRO.Utils
{
    public static class Tools
    {

        public static double Feet_to_cm(double length) => Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(length, UnitTypeId.Centimeters);
        public static double Feet_to_m(double length) => Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(length, UnitTypeId.Meters);
        public static double Feet2_to_m2(double length) => Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(length, UnitTypeId.SquareMeters);
        public static double Feet3_to_m3(double length) => Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(length, UnitTypeId.CubicMeters);
        public static double Feet_to_mm(double length) => Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(length, UnitTypeId.Millimeters);
        public static double Cm_to_Feet(double length) => Autodesk.Revit.DB.UnitUtils.ConvertToInternalUnits(length, UnitTypeId.Centimeters);
        public static double m_to_Feet(double length) => Autodesk.Revit.DB.UnitUtils.ConvertToInternalUnits(length, UnitTypeId.Meters);
        public static double mm_to_Feet(double length) => Autodesk.Revit.DB.UnitUtils.ConvertToInternalUnits(length, UnitTypeId.Millimeters);


        
    }

}