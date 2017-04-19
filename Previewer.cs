using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;
namespace NG
{
    class Previewer : Rhino.Display.DisplayConduit
    {
        public List<Curve> reg = new List<Curve>();
        public List<Curve> model1 = new List<Curve>();
        public List<Curve> model2 = new List<Curve>();
        public List<Point3d> points = new List<Point3d>();
        public List<Point3d> balancedpoints = new List<Point3d>();
        public double value1 = 0;
        public double value2 = 0;
        protected override void PostDrawObjects(DrawEventArgs e)
        {
            foreach (var m in balancedpoints)
            {
                e.Display.DrawPoint(m, PointStyle.X, 5, System.Drawing.Color.Red);
            }
            foreach (var m in points)
            {
                e.Display.DrawPoint(m, PointStyle.X,5, System.Drawing.Color.Green);
            }
            foreach (var m in reg)
            {
                e.Display.DrawCurve(m, System.Drawing.Color.Red);
            }
            foreach (var m in model1)
            {
                e.Display.DrawCurve(m,System.Drawing.Color.Blue);
            }

            foreach (var m in model2)
            {
                e.Display.DrawCurve(m, System.Drawing.Color.Gold);
            }

            
            Point3d point1 = new BoundingBox(model2.Select(n => n.PointAtNormalizedLength(0.5))).Center;
            e.Display.DrawDot(point1, value1.ToString(),System.Drawing.Color.Blue,System.Drawing.Color.White);
            
            if (model2.Count != 0)
            {
                Point3d point2 = new BoundingBox(model2.Select(n => n.PointAtNormalizedLength(0.5))).Max;
                e.Display.DrawDot(point2, value2.ToString(), System.Drawing.Color.Gold, System.Drawing.Color.Black);
            }
        }
    }
}
