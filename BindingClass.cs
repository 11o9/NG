using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
namespace NG.Class
{
    public class BindingClass
    {
        public List<string> omg { get; set; } = new List<string>() { "1종 일반", "2종 일반", "3종 일반", "상업" };


    }

    public class PlotSetting
    {
        //plottype = 용도지역
        public List<PlotTypes> PlotType { get; set; } = new List<PlotTypes>() { PlotTypes.제1종일반주거지역,PlotTypes.제2종일반주거지역,PlotTypes.제3종일반주거지역,PlotTypes.상업지역};
        public List<double> LegalFAR { get; set; } = new List<double>() { 150, 200, 250, 1300 };
        public List<double> LegalCVR { get; set; } = new List<double>() { 60, 60, 50, 80 };
        public List<int> LegalMaxF { get; set; } = new List<int>() { 4, 7, 10, 30 };

        public List<double> Roads { get; set; } = new List<double> { 4 };
    }

    public class BuildingSetting
    {
        //buildingtype =  건물유형
        //........
        public List<string> BuildingType { get; set; } = new List<string>() { "단독/다가구", "다세대" };
        public List<double> Near_Plot { get; set; } = new List<double>() { 1, 1.5 };
        public List<double> Near_Road { get; set; } = new List<double>() { 0.75, 1.5 };

    }

    public class UnitSettings
    {
        public List<string> TypeName { get; set; } = new List<string>() { "30m2형", "45m2형", "59m2형", "76m2형", "84m2형", "103m2형" };
        public List<double> ExclusiveArea { get; set; } = new List<double>() { 30, 45, 59, 76, 84, 103 };
        public List<double> Area { get; set; }
        public List<double> MinWidth { get; set; }
        public List<double> MaxWidth { get; set; }


        public UnitSettings()
        {
            Area = ExclusiveArea.Select(n => n * 1.5).ToList();
            MinWidth = new List<double>() { 3, 4, 5, 6, 7, 8 };
            MaxWidth = new List<double>() { 8, 9, 10, 11, 12, 13 };
        }
    }

    public class UnitSetting
    {
        public string TypeName { get; set; }
        public double ExclusiveArea { get; set; }
        public double Area { get; set; }
        public double MinWidth { get; set; }
        public double MaxWidth { get; set; }
        public double Rate { get; set; }
        public double CoreArea { get; set; }
        public double CorridorArea { get; set; }
        //simple ver

    }

    public class Plot
    {
        public PlotTypes PlotType { get; set; }
        public List<double> RoadWidths { get; set; }
        public double LegalFAR { get; set; }
        public double LegalCVR { get; set; }
        public int LegalMaxF { get; set; }
        public Curve Boundary { get; set; }
        public BuildingTypes BuildingType {get;set;}
        public double Area { get { if (Boundary == null) return 0; else return AreaMassProperties.Compute(Boundary).Area; } }
    }

}
