using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Geometry;
using NG.Class;
namespace NG
{
    public enum PlotTypes
    {
        제1종일반주거지역,
        제2종일반주거지역,
        제3종일반주거지역,
        상업지역
    }

    public enum BuildingTypes
    {
        단독다가구,
        다세대
    }

    class Regulation
    {
       

        //도로 중심선
        public static Curve RoadCenterLines(Plot plot)
        {
            //plot 의 boundary 와 roads 를 사용.
            var segments = plot.Boundary.DuplicateSegments();
            var roadwidths = plot.RoadWidths.Select(n=>n*0.5).ToList();

            for (int i = 0; i < segments.Length; i++)
            {
                int j = i % roadwidths.Count;
                Curve temp = segments[i];
                var v = temp.TangentAtStart;
                v.Rotate(Math.PI / 2, Vector3d.ZAxis);
                temp.Translate(v * roadwidths[j]);
                segments[i] = temp;
            }

            List<Point3d> topoly = new List<Point3d>();
            for (int k = 0; k < segments.Length; k++)
            {
                int j = (k + 1) % segments.Length;
                Line li = new Line(segments[k].PointAtStart, segments[k].PointAtEnd);
                Line lj = new Line(segments[j].PointAtStart, segments[j].PointAtEnd);
                double paramA;
                double paramB;
                var intersect = Rhino.Geometry.Intersect.Intersection.LineLine(li, lj, out paramA, out paramB, 0, false);

                topoly.Add(li.PointAt(paramA));
            }

            topoly.Add(topoly[0]);

            return new Polyline(topoly).ToNurbsCurve();

        }

        //정북방향
        public static List<Curve> North(Curve roadCenter, Plot plot)
        {
            List<Curve[]> copy = new List<Curve[]>();
            double t = -1;
            if (UseNorth(plot))
            {
                //move
            }

            else
            {
                t = 0;
            }


            for (int i = 0; i < plot.LegalMaxF; i++)
            {
                
                double tempHeight = 3.3 + i * 2.8;//숫자 -> 나중에 변수로

                var tempRoadLine = roadCenter.DuplicateCurve();
                if (tempHeight < 9)//숫자 -> 나중에 변수로
                    tempRoadLine.Translate(Vector3d.YAxis * t * 1.5);//숫자 -> 나중에 변수로
                else
                    tempRoadLine.Translate(Vector3d.YAxis * t * tempHeight / 2);//숫자 -> 나중에 변수로

                var newCurve = Curve.CreateBooleanIntersection(tempRoadLine, plot.Boundary);
                for (int j = 0; j < newCurve.Length; j++)
                    newCurve[j].Translate(Vector3d.ZAxis * tempHeight);

                copy.Add(newCurve);
            }

            return copy.Select(n => n[0]).ToList();
            //plot의 boundary, maxfloor, roadwidths, plottype 사용 , 도로중심선
        }
        public static bool UseNorth(Plot plot)
        {
            List<double> roadwidths = plot.RoadWidths;
            PlotTypes plotinfo = plot.PlotType;
            if (roadwidths.Max() >= 20)
                return false;
            else if ((int)plotinfo == 3)
                return false;
            else
                return true;


        }

        //대지안의공지
        public static List<Curve> Clearance(Plot plot)
        {
            //plot의 boundary, maxfloor, roadwidth, buildingtype 사용
            var segments = plot.Boundary.DuplicateSegments();
            var roadwidths = plot.RoadWidths;
            double plotoffset = plot.BuildingType == BuildingTypes.다세대 ? 1.5 : 1;
            double roadoffset = plot.BuildingType == BuildingTypes.단독다가구 ? 1.5 : 0.75;

            for (int i = 0; i < segments.Length; i++)
            {
                int j = i % roadwidths.Count;
                Curve temp = segments[i];
                var v = temp.TangentAtStart;
                v.Rotate(Math.PI / 2, Vector3d.ZAxis);
                if (roadwidths[j] == 0)
                    temp.Translate(-v * plotoffset);
                else
                    temp.Translate(-v * roadoffset);

                segments[i] = temp;
            }

            List<Point3d> topoly = new List<Point3d>();
            for (int k = 0; k < segments.Length; k++)
            {
                int j = (k + 1) % segments.Length;
                Line li = new Line(segments[k].PointAtStart, segments[k].PointAtEnd);
                Line lj = new Line(segments[j].PointAtStart, segments[j].PointAtEnd);
                double paramA;
                double paramB;
                var intersect = Rhino.Geometry.Intersect.Intersection.LineLine(li, lj, out paramA, out paramB, 0, false);

                topoly.Add(li.PointAt(paramA));
            }

            topoly.Add(topoly[0]);


            List<Curve> result = new List<Curve>();
            for (int i = 0; i < plot.LegalMaxF; i++)
            {
                double height = 3.3 + 2.8 * i;
                Polyline poly = new Polyline(topoly);
                poly.Transform(Transform.Translation(Vector3d.ZAxis * height));
                result.Add(poly.ToNurbsCurve());
            }
            return result;
        }

        //채광방향
        public static List<Curve> Lighting(Curve roadCenter, Plot plot, double aptAngle)
        {

            double d = 0;
            if (plot.PlotType != PlotTypes.상업지역)
            {
                if (plot.LegalMaxF <= 7)
                    d = 0.25;
                else
                    d = 0.5;
            }
            else
                d = 0.25;

            Curve basecurve = null;
            var cp = AreaMassProperties.Compute(plot.Boundary).Centroid;
            var basev = Vector3d.XAxis;
            basev.Rotate(aptAngle, Vector3d.ZAxis);
            var bounding = plot.Boundary.GetBoundingBox(false);
            basecurve = new LineCurve(cp - basev * bounding.Diagonal.Length / 2, cp + basev * bounding.Diagonal.Length / 2);

            List<Curve> result = new List<Curve>();
            Curve last = roadCenter.DuplicateCurve();
            double pheight = 3.3;
            double fheight = 2.8;
            int floor = plot.LegalMaxF;

            List<Curve> debug = new List<Curve>();

            for (int i = 0; i < floor; i++)
            {
                var height = pheight + fheight * i;
                var distance = d * (height - pheight);

                var segments = roadCenter.DuplicateSegments();
                for (int j = 0; j < segments.Length; j++)
                {
                    var ps = segments[j].PointAtStart;
                    var pe = segments[j].PointAtEnd;

                    double ds = 0;
                    double de = 0;

                    
                    basecurve.ClosestPoint(ps, out ds);
                    basecurve.ClosestPoint(pe, out de);

                    Vector3d vs = basecurve.PointAt(ds) - ps;
                    Vector3d ve = basecurve.PointAt(de) - pe;
                    vs.Unitize();
                    ve.Unitize();

                    var mp = segments[j].PointAtNormalizedLength(0.5);
                    var ts = mp + vs;
                    var te = mp + ve;

                    if (roadCenter.Contains(ts) == PointContainment.Inside)
                    {
                        segments[j].Translate(vs * distance);
                    }
                    else if (roadCenter.Contains(te) == PointContainment.Inside)
                    {
                        segments[j].Translate(ve * distance);
                    }

                    segments[j].Translate(Vector3d.ZAxis * height);
                }

                List<Point3d> topoly = new List<Point3d>();
                for (int k = 0; k < segments.Length; k++)
                {
                    int j = (k + 1) % segments.Length;
                    Line li = new Line(segments[k].PointAtStart, segments[k].PointAtEnd);
                    Line lj = new Line(segments[j].PointAtStart, segments[j].PointAtEnd);
                    double paramA;
                    double paramB;
                    var intersect = Rhino.Geometry.Intersect.Intersection.LineLine(li, lj, out paramA, out paramB, 0, false);

                    topoly.Add(li.PointAt(paramA));
                }

                topoly.Add(topoly[0]);
                var tempcurve = new PolylineCurve(topoly);
                var rotation = tempcurve.ClosedCurveOrientation(Vector3d.ZAxis);
                var selfintersection = Rhino.Geometry.Intersect.Intersection.CurveSelf(tempcurve, 0);

                var parameters = selfintersection.Select(n => n.ParameterA).ToList();

                parameters.AddRange(selfintersection.Select(n => n.ParameterB));

                var spl = tempcurve.Split(parameters);

                var f = NewJoin(spl);


                var merged = f.Where(n => n.ClosedCurveOrientation(Vector3d.ZAxis) == rotation).ToList();
                debug.AddRange(merged);


                for (int j = merged.Count - 1; j >= 0; j--)
                {
                    var tc = merged[j].DuplicateCurve();
                    tc.Translate(Vector3d.ZAxis * -tc.PointAtStart.Z);

                    if (Curve.CreateBooleanDifference(tc, last).Length == 0)
                    {
                        last = tc;
                        result.Add(merged[j]);
                    }
                }
            }

            return result;
        }
        public static List<Curve> NewJoin(IEnumerable<Curve> spl)
        {
            Queue<Curve> q = new Queue<Curve>(spl);
            Stack<Curve> s = new Stack<Curve>();
            List<Curve> f = new List<Curve>();
            while (q.Count > 0)
            {
                Curve temp = q.Dequeue();

                if (temp.IsClosable(0))
                {
                    temp.MakeClosed(0);
                    f.Add(temp);
                }

                else
                {
                    if (s.Count > 0)
                    {
                        Curve pop = s.Pop();
                        var joined = Curve.JoinCurves(new Curve[] { pop, temp });
                        if (joined[0].IsClosable(0))
                        {
                            joined[0].MakeClosed(0);
                            f.Add(joined[0]);
                        }
                        else
                        {
                            s.Push(pop);
                            s.Push(temp);
                        }
                    }
                    else
                    {
                        s.Push(temp);
                    }
                }
            }

            if (s.Count > 0)
            {
                var last = Curve.JoinCurves(s);
                f.AddRange(last);
            }
            return f;
        }
        //합체!
        public static List<Curve> MergedRegulation(Plot plot, double angle)
        {
            var roadCenter = RoadCenterLines(plot);
            var north = North(roadCenter, plot);
            var clearance = Clearance(plot);
            var lighting = Lighting(roadCenter, plot, angle);

            List<Curve> result = new List<Curve>();

            int minimum = north.Count < clearance.Count ? north.Count : clearance.Count;
            minimum = minimum < lighting.Count ? minimum : lighting.Count;

            for (int i = 0; i < minimum; i++)
            {
                var a = Curve.CreateBooleanIntersection(north[i], clearance[i]);
                var b = Curve.CreateBooleanIntersection(a[0], lighting[i]);
                if (b.Length > 0)
                    result.Add(b[0]);
            }

            return result;
        }

        public static List<Curve> MergedRegulation(List<Curve> a, Plot plot, double angle)
        {
            var roadCenter = RoadCenterLines(plot);
            var lighting = Lighting(roadCenter, plot, angle);

            List<Curve> result = new List<Curve>();

            int minimum = a.Count < lighting.Count ? a.Count : lighting.Count;

            for (int i = 0; i < minimum; i++)
            {
                var b = Curve.CreateBooleanIntersection(a[i], lighting[i]);
                if (b.Length > 0)
                    result.Add(b[0]);
            }

            return result;
        }

        public static List<Curve> MergedRegulation_TopOnly(List<Curve> a, Plot plot, double angle, out int maxfloor)
        {
            var roadCenter = RoadCenterLines(plot);
            var lighting = Lighting(roadCenter, plot, angle);

            List<Curve> result = new List<Curve>();

            int minimum = a.Count < lighting.Count ? a.Count : lighting.Count;

            int temporary = 0;

            for (int i = minimum - 1; i >= 0; i--)
            {
                var b = Curve.CreateBooleanIntersection(a[i], lighting[i]);
                if (b.Length > 0)
                {
                    result.Add(b[0]);
                    temporary = i+1;
                    break;
                }
            }

            maxfloor = temporary;
            return result;
        }
    }
}
