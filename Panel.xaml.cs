using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NG.Class;
using Rhino.Geometry;
using System.Threading.Tasks;
using System.Threading;
namespace NG
{
    /// <summary>
    /// Panel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Panel : UserControl
    {

        Curve tempSelectedBoundary = null;
        PlotSetting setting = new PlotSetting();
        Brush[] brushes = new Brush[] { Brushes.Gold, Brushes.Aqua, Brushes.Beige, Brushes.Magenta };
        Previewer preview = new Previewer();
        UnitSettings unitSettings = new UnitSettings();
        List<UnitSetting> units = new List<UnitSetting>();

        public Panel()
        {
            InitializeComponent();
            preview.Enabled = true;
            UnitSelection.ItemsSource = unitSettings.TypeName;
            //Units.ItemsSource = units;
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (PlotTypeCombo.SelectedIndex == -1)
                return;

            if (tempSelectedBoundary == null)
                return;

            if (Width_Combo.SelectedIndex == -1)
                return;
            Graph.Children.Clear();
            Angle_Combo.Items.Clear();
            AddPlotAngle();
            Generate_SelectedType();
            //Generate_Graph();
        }
        private void AddPlotAngle()
        {
            var segments = tempSelectedBoundary.DuplicateSegments();
            var vectors = segments.Select(n => n.TangentAtStart);

            var acos = vectors.Select(n => n.Y / Math.Abs(n.Y == 0 ? 1 : n.Y) * Math.Acos(n.X));
            var angles = acos.Select(n => n * 180 / Math.PI);
            angles.ToList().ForEach(n => Angle_Combo.Items.Add(n));
        }
        private async void Generate_SelectedType()
        {
            Generate.IsEnabled = false;
            //GenerateOne.IsEnabled = false;
            int selected = PlotTypeCombo.SelectedIndex;
            double aptwidth = (double)Width_Combo.SelectedItem;
            double canvasy = Graph.Height;
            double canvasx = Graph.Width;
            var task1 = Task<List<System.Windows.Point>>.Run(() => CalcAsync(selected, aptwidth));
            List<System.Windows.Point> points = await task1;
            int maxindex = 0;
            List<System.Windows.Shapes.Ellipse> els = new List<System.Windows.Shapes.Ellipse>();
            for (int p = 0; p < points.Count; p++)
            {
                double width = 2;
                double height = 2;
                double newyvalue = canvasy - points[p].Y;
                //newyvalue *= ;
                var ne = new System.Windows.Shapes.Ellipse();
                ne.Margin = new Thickness((points[p].X * 2) - width / 2, newyvalue - height / 2, 0, 0);
                ne.Width = width;
                ne.Height = height;
                ne.Fill = brushes[selected];
                els.Add(ne);
                if (points[p].Y > points[maxindex].Y)
                    maxindex = p;
            }
            List<int> items = new List<int>();
            for (int j = 1; j < points.Count-1; j++)
            {
                int i = j - 1;
                int k = j + 1;
                //가운데가제일크면
                if (points[j].Y > points[i].Y && points[j].Y > points[k].Y)
                {
                    els[j].Fill = Brushes.HotPink;
                    items.Add(j);
                } 
            }
            els.ForEach(n => Graph.Children.Add(n));
            items.ForEach(n => Angle_Combo.Items.Add((double)n));
            //Angle_Combo.ItemsSource = items;
            Generate.IsEnabled = true;
            //GenerateOne.IsEnabled = true;
        }
        private async void Generate_Graph()
        {
            Generate.IsEnabled = false;
            //GenerateOne.IsEnabled = false;

            double aptwidth = (double)Width_Combo.SelectedItem;
            
            double canvasy = Graph.Height;
            double canvasx = Graph.Width;
            //int i = PlotTypeCombo.SelectedIndex;

            int selected = PlotTypeCombo.SelectedIndex;

            var task1 = Task<List<List<System.Windows.Point>>>.Run(() => CalcAsyncEachFloor(selected, aptwidth));

            List<List<System.Windows.Point>> points = await task1;

            //var task1 = Task<List<System.Windows.Point>>.Run(() => CalcAsync(i, aptwidth));
            //List<System.Windows.Point> points = await task1;
                


            int maxindex = 0;

            //Rhino.RhinoApp.WriteLine(((PlotTypes)sum).ToString() + "완료");
            //ResultMax.Text = "최대각 : " + maxindex.ToString() + ", 값 : " + points[maxindex].Y.ToString();
            for (int p = 0; p < points.Count; p++)
            {
                for (int i = 0; i < 4; i++)
                {
                    double width = 2;
                    double height = 2;
                    double newyvalue = canvasy - points[p][i].Y;
                    //newyvalue *= ;

                    var ne = new System.Windows.Shapes.Ellipse();
                    ne.Margin = new Thickness((points[p][i].X * 2) - width / 2, newyvalue - height / 2, 0, 0);
                    ne.Width = width;
                    ne.Height = height;
                    ne.Fill = brushes[i];

                    Graph.Children.Add(ne);

                    if (points[p][i].Y > points[maxindex][i].Y)
                        maxindex = p;
                }
            }

            Generate.IsEnabled = true;
            //GenerateOne.IsEnabled = true;
        }
        private List<System.Windows.Point> CalcAsync(int i,double width)
        {
            
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            Plot plot = new Plot() { Boundary = tempSelectedBoundary, BuildingType = BuildingTypes.다세대, LegalCVR = setting.LegalCVR[i], LegalFAR = setting.LegalFAR[i], LegalMaxF = setting.LegalMaxF[i], PlotType = (PlotTypes)i, RoadWidths = new List<double>() { 4 } };

            //test
            var roadCenter = Regulation.RoadCenterLines(plot);
            var north = Regulation.North(roadCenter, plot);
            var clearance = Regulation.Clearance(plot);
            int minimum = north.Count > clearance.Count ? clearance.Count : north.Count;
            List<Curve> nc = new List<Curve>();
            for (int j = 0; j < minimum; j++)
            {
                var a = Curve.CreateBooleanIntersection(north[j], clearance[j]);
                nc.Add(a[0]);
            }


            //List<List<Curve>> curvesCollection = new List<List<Curve>>();
            List<double> valuesCollection = new List<double>();
            var start = DateTime.Now;
            for (int r = 0; r < 180; r++)
            {
                int maxfloor;
                var reg = Regulation.MergedRegulation_TopOnly(nc, plot, (double)r / 180 * Math.PI, out maxfloor);

                List<Curve> trash;
                List<Rhino.Geometry.Line> trash2;
                var length = Calculate.AngleMax_Simple(reg.Last(), maxfloor, width, (double)r / 180 * Math.PI, out trash, out trash2);

                valuesCollection.Add(length);
                points.Add(new System.Windows.Point(r, length * maxfloor / 10));
            }
            var end = DateTime.Now;
            int endsec = end.Second;
            int startsec = start.Second;
            if (startsec > endsec)
                endsec += 60;
            double endtime = endsec * 1000 + end.Millisecond;
            double starttime = startsec * 1000 + start.Millisecond;
            double x = (endtime - starttime) / 1000;
            Rhino.RhinoApp.WriteLine("180회 소요시간 " + x.ToString() + "초, 회당 " + (x / 180).ToString() + "초");
            double lengthmax = points.Select(n => n.Y).Max();

            return points;
        }

        private List<List<System.Windows.Point>> CalcAsyncEachFloor(int i, double width)
        {
            List<List<System.Windows.Point>> result = new List<List<System.Windows.Point>>();
            Plot plot = new Plot() { Boundary = tempSelectedBoundary, BuildingType = BuildingTypes.다세대, LegalCVR = setting.LegalCVR[i], LegalFAR = setting.LegalFAR[i], LegalMaxF = setting.LegalMaxF[i], PlotType = (PlotTypes)i, RoadWidths = new List<double>() { 4 } };

            //test
            var roadCenter = Regulation.RoadCenterLines(plot);
            var north = Regulation.North(roadCenter, plot);
            var clearance = Regulation.Clearance(plot);
            int minimum = north.Count > clearance.Count ? clearance.Count : north.Count;
            List<Curve> nc = new List<Curve>();
            for (int j = 0; j < minimum; j++)
            {
                var a = Curve.CreateBooleanIntersection(north[j], clearance[j]);
                nc.Add(a[0]);
            }


            //List<List<Curve>> curvesCollection = new List<List<Curve>>();
            //List<double> valuesCollection = new List<double>();
            var start = DateTime.Now;
            for (int r = 0; r < 180; r++)
            {
                List<System.Windows.Point> points = new List<System.Windows.Point>();

                var reg = Regulation.MergedRegulation(nc, plot, (double)r / 180 * Math.PI);

                List<Curve> trash;
                List<Rhino.Geometry.Line> trash2;
                for (int f = reg.Count-4; f < reg.Count; f++)
                {
                    var length = Calculate.AngleMax_Simple(reg[f], f, width, (double)r / 180 * Math.PI, out trash,out trash2);

                    //valuesCollection.Add(length);
                    points.Add(new System.Windows.Point(r, length * f / 10));
                }

                result.Add(points);
            }
            var end = DateTime.Now;
            int endsec = end.Second;
            int startsec = start.Second;
            if (startsec > endsec)
                endsec += 60;
            double endtime = endsec * 1000 + end.Millisecond;
            double starttime = startsec * 1000 + start.Millisecond;
            double x = (endtime - starttime) / 1000;
            Rhino.RhinoApp.WriteLine("소요시간 " + x.ToString() + "초");
            //double lengthmax = points.Select(n => n.Y).Max();

            return result;
        }
        private void SelCurve_Click(object sender, RoutedEventArgs e)
        {
        //    Rhino.Geometry.Polyline result;
        //    var r = Rhino.Input.RhinoGet.GetPolyline(out result);
        //    if (r == Rhino.Commands.Result.Success)
        //    {
        //        Curve temp = result.ToNurbsCurve();
        //        if (result.ToNurbsCurve().ClosedCurveOrientation(Plane.WorldXY) != CurveOrientation.CounterClockwise)
        //            temp.Reverse();
        //        tempSelectedBoundary = temp;
        //    }

            Rhino.DocObjects.ObjRef result;
            var r = Rhino.Input.RhinoGet.GetOneObject("커브 선택", true, Rhino.DocObjects.ObjectType.Curve, out result);
            if (r == Rhino.Commands.Result.Success)
            {
                Curve temp = result.Curve();
                if (temp.ClosedCurveOrientation(Plane.WorldXY) != CurveOrientation.CounterClockwise)
                    temp.Reverse();
                tempSelectedBoundary = temp;
            }
        }

        //private void GenerateOne_Click(object sender, RoutedEventArgs e)
        //{
        //    if (PlotTypeCombo.SelectedIndex == -1)
        //        return;

        //    if (tempSelectedBoundary == null)
        //        return;

        //    int i = PlotTypeCombo.SelectedIndex;

        //    Plot plot = new Plot() { Boundary = tempSelectedBoundary, BuildingType = BuildingTypes.다세대, LegalCVR = setting.LegalCVR[i], LegalFAR = setting.LegalFAR[i], LegalMaxF = setting.LegalMaxF[i], PlotType = (PlotTypes)i, RoadWidths = new List<double>() { 4 } };


        //    //test
        //    var roadCenter = Regulation.RoadCenterLines(plot);
        //    var north = Regulation.North(roadCenter, plot);
        //    var clearance = Regulation.Clearance(plot);
        //    int minimum = north.Count > clearance.Count ? clearance.Count : north.Count;
        //    List<Curve> nc = new List<Curve>();
        //    for (int j = 0; j < minimum; j++)
        //    {
        //        var a = Curve.CreateBooleanIntersection(north[j], clearance[j]);
        //        nc.Add(a[0]);
        //    }

        //    int r = (int)Angle.Value;

        //    var start = DateTime.Now;
        //    var reg = Regulation.MergedRegulation(nc, plot, (double)r / 180 * Math.PI);
        //    var end = DateTime.Now;
        //    //Rhino.RhinoApp.WriteLine("법규선계산 : " + (end.Second - start.Second) + "초" + (end.Millisecond - start.Millisecond) + "ms");
        //    start = DateTime.Now;
        //    List<Curve> temp1;
        //    List<Curve> temp2;
        //    var val1 = Calculate.AngleMax_Josh(reg.Last(), plot.LegalMaxF, 9, (double)r / 180 * Math.PI,out temp1);
        //    end = DateTime.Now;
        //    //Rhino.RhinoApp.WriteLine("최대길이계산 : " + (end.Second - start.Second) + "초" + (end.Millisecond - start.Millisecond) + "ms  yvalue = " + Math.Round(length, 1).ToString());
        //    var val2 = Calculate.AngleMax(reg.Last(), plot.LegalMaxF, 9, (double)r / 180 * Math.PI, out temp2);
        //    //points.Add(new System.Windows.Point(r, length));



        //    //double lengthmax = points.Select(n => n.Y).Max();

        //    preview.reg = reg;
        //    preview.model1 = temp1;
        //    preview.model2 = temp2;
        //    preview.value1 = val1;
        //    preview.value2 = val2;
        //    Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.Redraw();
        //}

            /// <summary>
            /// asdfasfdfdassfdasfda
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
        private void Angle_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Angle_Combo.SelectedIndex == -1)
                return;

            if (PlotTypeCombo.SelectedIndex == -1)
                return;

            if (tempSelectedBoundary == null)
                return;

            if (Width_Combo.SelectedIndex == -1)
                return;

            int i = PlotTypeCombo.SelectedIndex;
            //폭
            double width = (double)Width_Combo.SelectedItem;
            double canvasy = Graph.Height;
            double canvasx = Graph.Width;
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            Plot plot = new Plot() { Boundary = tempSelectedBoundary, BuildingType = BuildingTypes.다세대, LegalCVR = setting.LegalCVR[i], LegalFAR = setting.LegalFAR[i], LegalMaxF = setting.LegalMaxF[i], PlotType = (PlotTypes)i, RoadWidths = new List<double>() { 4 } };


            //test
            var roadCenter = Regulation.RoadCenterLines(plot);
            var north = Regulation.North(roadCenter, plot);
            var clearance = Regulation.Clearance(plot);
            int minimum = north.Count > clearance.Count ? clearance.Count : north.Count;
            List<Curve> nc = new List<Curve>();
            for (int j = 0; j < minimum; j++)
            {
                var a = Curve.CreateBooleanIntersection(north[j], clearance[j]);
                nc.Add(a[0]);
            }





            double r = (double)Angle_Combo.SelectedValue;

            var start = DateTime.Now;
            var reg = Regulation.MergedRegulation(nc, plot, (double)r / 180 * Math.PI);
            var end = DateTime.Now;
            //Rhino.RhinoApp.WriteLine("법규선계산 : " + (end.Second - start.Second) + "초" + (end.Millisecond - start.Millisecond) + "ms");
            start = DateTime.Now;
            //List<Curve> temp1;
            List<Curve> temp2;
            //var val1 = Calculate.AngleMax_Josh(reg.Last(), plot.LegalMaxF, 9, (double)r / 180 * Math.PI, out temp1);
            end = DateTime.Now;

            List<Rhino.Geometry.Line> parkingBaseLine;
            //Rhino.RhinoApp.WriteLine("최대길이계산 : " + (end.Second - start.Second) + "초" + (end.Millisecond - start.Millisecond) + "ms  yvalue = " + Math.Round(length, 1).ToString());
            var val2 = Calculate.AngleMax_Simple(reg.Last(), plot.LegalMaxF, width, (double)r / 180 * Math.PI, out temp2, out parkingBaseLine);

            double aptDistance = (reg.Last().PointAtStart.Z - 3.3) * 0.8;
            double linedistance = width + aptDistance;

            var far = Math.Round(val2 * width * plot.LegalMaxF / plot.Area,4) * 100;
            //points.Add(new System.Windows.Point(r, length));


            //유닛분배........................................................................................
            //유닛정보,라인..음..
            List<Point3d> aptSeparatePoints = new List<Point3d>();
            List<Point3d> balancedPoints = new List<Point3d>();
            for (int j = 0; j < temp2.Count; j++)
            {
                var tempcurve = temp2[j];
                var templength = tempcurve.GetLength();
                var unitset = (UnitSetting)Units.Items[0];
                var unitlength = unitset.Area / width;
                var p1 = tempcurve.PointAtLength(templength);
                aptSeparatePoints.Add(p1);
                int aptcount = 0;
                while (templength > 2*unitlength)
                {
                    var p2 = tempcurve.PointAtLength(templength - unitlength);
                    var p3 = tempcurve.PointAtLength(templength - unitlength * 2);
                    aptcount += 2;
                    aptSeparatePoints.Add(p2);
                    aptSeparatePoints.Add(p3);
                    templength -= 2 * unitlength;
                }

                double lengthleftperapt = templength / aptcount;
                double balancedaptlength = unitlength + lengthleftperapt;
                for (int k = 0; k <= aptcount; k++)
                {
                    var temppoint = tempcurve.PointAtLength(k * balancedaptlength);
                    balancedPoints.Add(temppoint);
                }
            }

            preview.points = aptSeparatePoints;
            preview.balancedpoints = balancedPoints;


            List<Curve> parkingLinesCollection = new List<Curve>();

            for (int p = 0; p < parkingBaseLine.Count; p++)
            {
                bool addfirst = p == 0 ? true : false;
                List<Curve> parkingLines = ParkingPrediction.Calculate(linedistance, parkingBaseLine[p].ToNurbsCurve(), addfirst);
                parkingLinesCollection.AddRange(parkingLines);
            }
            
            
            //주차는.?

            //for()

            //plot, aptline, width, distance

            //distance 와 set

            //double lengthmax = points.Select(n => n.Y).Max();

            preview.reg = reg;
            preview.model1 = parkingLinesCollection;
            preview.model2 = temp2;
            preview.value1 = linedistance;
            preview.value2 = far;
            Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.Redraw();
        }
        //add 
        private void UnitSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UnitSelection.SelectedIndex == -1)
                return;

            int i = UnitSelection.SelectedIndex;

            UnitSetting tempsetting = new UnitSetting() { Area = unitSettings.Area[i], TypeName = unitSettings.TypeName[i], ExclusiveArea = unitSettings.ExclusiveArea[i], MinWidth = unitSettings.MinWidth[i], MaxWidth = unitSettings.MaxWidth[i] };
            UnitSetting[] temp = new UnitSetting[Units.Items.Count];
            Units.Items.CopyTo(temp, 0);


            if (temp.Where(n => n.TypeName == tempsetting.TypeName).ToList().Count != 0)
                return;
           
            int index = temp.Where(n => n.Area < tempsetting.Area).Count();

            Units.Items.Insert(index, tempsetting);

            UnitSelection.SelectedIndex = -1;

            UnitSetup();
        }
        //remove
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is UnitSetting)
            {
                UnitSetting deleteme = (UnitSetting)btn.DataContext;
                Units.Items.Remove(deleteme);
            }

            UnitSetup();
        }
        //set
        private void UnitSetup()
        {
            Width_Combo.Items.Clear();
            UnitSetting[] temp = new UnitSetting[Units.Items.Count];
            Units.Items.CopyTo(temp, 0);

            if (temp.Length == 0)
            {
                //선택 x ?
                return;
            }


            //최대값의최소값
            double widthMax = temp.Select(n => n.MaxWidth).Min();
            //최소값의최대값
            double widthMin = temp.Select(n => n.MinWidth).Max();
            //최대값의 최소값, 최소값의 최대값 의 평균
            double widthAvrg = (widthMax + widthMin) / 2;
           
            Width_Combo.Items.Add(widthMin);
            Width_Combo.Items.Add(widthAvrg);
            Width_Combo.Items.Add(widthMax);

        }
    }
}
