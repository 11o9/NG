using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NG
{
    public class ParkingPrediction
    {
        public static List<Curve> Calculate(double initialLength, Curve initialCurve, bool additional)
        {
            PeterParker origin = new PeterParker(initialLength, initialCurve);
            Queue<PeterParker> wait = new Queue<PeterParker>();
            PeterParkerCollection fit = new PeterParkerCollection();
            wait.Enqueue(origin);

            while (wait.Count > 0)
            {
                PeterParker current = wait.Dequeue();
                for (int i = 0; i < (int)ParkingType.Max; i++)
                {
                    PeterParker temp = new PeterParker(current, (ParkingType)i);
                    if (temp.LeftLength() < 0)
                    {
                        //cull
                    }
                    else if (temp.LeftLength() > 0)
                    {
                        if (temp.LeftLength() < 5)
                        {
                            //fit
                            fit.Add(temp);
                        }
                        else
                        {
                            //re enqueue
                            wait.Enqueue(temp);
                        }
                    }

                }
            }


            //get parking line test

            List<double> offsets = fit.collection.Last().OffsetDistances();

            offsets.Insert(0, 0);
            List<Curve> results = new List<Curve>();
            //results.Add(initialCurve.DuplicateCurve());
            for (int i = 0; i < offsets.Count; i++)
            {
                Curve temp = initialCurve.DuplicateCurve();
                Vector3d v = temp.TangentAtStart;
                v.Rotate(Math.PI / 2, Vector3d.ZAxis);
                temp.Translate(v * (offsets[i]));
                results.Add(temp);
            }

            if (additional)
            {
                for (int i = 0; i < offsets.Count; i++)
                {
                    Curve temp = initialCurve.DuplicateCurve();
                    Vector3d v = temp.TangentAtStart;
                    v.Rotate(Math.PI / 2, Vector3d.ZAxis);
                    temp.Translate(v * (offsets[i] - initialLength));
                    results.Add(temp);
                }
            }



            List<Parking> fitParks = fit.collection.Last().parkings;
            List<Curve> partitions = new List<Curve>();
            int parkCount = 0;
            for (int i = 0; i < results.Count-1; i++)
            {
                int j = (i + 1) % results.Count;

                double tempLength = Math.Round(results[j].PointAtStart.DistanceTo(results[i].PointAtStart),3);
                for (int f = 0; f < fitParks.Count; f++)
                {
                    if (tempLength == fitParks[f].height)
                    {
                        var divideByLength = results[j].DivideByLength(fitParks[f].widthOffset, true);
                        Vector3d v = results[j].PointAtStart - results[i].PointAtStart;
                        //v.Rotate(Math.PI / 2, Vector3d.ZAxis);
                        for (int p = 0; p < divideByLength.Length; p++)
                        {
                            LineCurve partition = new LineCurve(new Line(results[i].PointAt(divideByLength[p]), v));
                            partitions.Add(partition);
                        }

                        parkCount++;
                        //if (parkCount > fitParks.Count - 1)
                        //    break;

                    }
                }
            }

            results.AddRange(partitions);

            return results;
        }
    }
    public class PeterParkerCollection
    {
        public List<PeterParker> collection;
        public int Count { get { return collection.Count; } }
        public PeterParkerCollection()
        {
            collection = new List<PeterParker>();
        }

        public void Add(PeterParker parker)
        {
            if (collection.Count < 10)
            {
                collection.Add(parker);
                Sort();
            }

            else
            {
                if (collection[0].ParkingUnitCount().Sum() < parker.ParkingUnitCount().Sum())
                {
                    collection.RemoveAt(0);
                    collection.Add(parker);
                    Sort();
                }
            }
        }

        void Sort()
        {
            PeterParkerComparer comp = new PeterParkerComparer();
            collection.Sort(comp);
        }
    }
    public enum ParkingType
    {
        P0Single,
        P0Double,
        P45Single,
        P45Double,
        P60Single,
        P60Double,
        P90Single,
        P90Double,
        Max
    }
    public class Parking
    {
        public double height;
        public double width;
        public double widthOffset;
        public double necessaryRoad;
        public bool isDouble;
        public Parking(ParkingType type)
        {
            switch (type)
            {
                case ParkingType.P0Single:
                    {
                        height = 2;
                        width = 6;
                        widthOffset = 6;
                        necessaryRoad = 3.5;
                        isDouble = false;
                        break;
                    }

                case ParkingType.P0Double:
                    {
                        height = 4;
                        width = 6;
                        widthOffset = 6;
                        necessaryRoad = 3.5;
                        isDouble = true;
                        break;
                    }

                case ParkingType.P45Single:
                    {
                        height = 5.162;
                        width = 5.162;
                        widthOffset = 3.253;
                        necessaryRoad = 3.5;
                        isDouble = false;
                        break;
                    }

                case ParkingType.P45Double:
                    {
                        height = 8.697;
                        width = 5.162;
                        widthOffset = 3.253;
                        necessaryRoad = 3.5;
                        isDouble = true;
                        break;
                    }

                case ParkingType.P60Single:
                    {
                        height = 5.48;
                        width = 4.492;
                        widthOffset = 2.656;
                        necessaryRoad = 4.5;
                        isDouble = false;
                        break;
                    }

                case ParkingType.P60Double:
                    {
                        height = 9.18;
                        width = 4.492;
                        widthOffset = 2.656;
                        necessaryRoad = 4.5;
                        isDouble = true;
                        break;
                    }

                case ParkingType.P90Single:
                    {
                        height = 5;
                        width = 2.3;
                        widthOffset = 2.3;
                        necessaryRoad = 6;
                        isDouble = false;
                        break;
                    }

                case ParkingType.P90Double:
                    {
                        height = 10;
                        width = 2.3;
                        widthOffset = 2.3;
                        necessaryRoad = 6;
                        isDouble = true;
                        break;
                    }

            }
        }

        public void GenerateParkingLines()
        {

        }
    }
    public class PeterParkerComparer : IComparer<PeterParker>
    {
        public int Compare(PeterParker a, PeterParker b)
        {

            return a.ParkingUnitCount().Sum().CompareTo(b.ParkingUnitCount().Sum());
        }

    }
    public class PeterParker
    {
        public List<Parking> parkings;
        public double totalLength;
        public Curve baseCurve;

        public PeterParker(double length, Curve baseCurve)
        {
            parkings = new List<Parking>();
            totalLength = length;
            this.baseCurve = baseCurve.DuplicateCurve();
        }

        public PeterParker(PeterParker parent, ParkingType type)
        {
            parkings = new List<Parking>(parent.parkings);
            parkings.Add(new Parking(type));
            totalLength = parent.totalLength;
            baseCurve = parent.baseCurve;
        }


        public List<double> OffsetDistances()
        {
            List<double> Lengths = new List<double>();

            double sum = 0;

            for (int i = 0; i < parkings.Count; i++)
            {
                int j = (i + 1) % parkings.Count;
                double roadi = parkings[i].necessaryRoad;
                double roadj = parkings[j].necessaryRoad;
                double roadLonger = roadi > roadj ? roadi : roadj;
                sum += parkings[i].height;
                Lengths.Add(sum);
                sum += roadLonger;
                Lengths.Add(sum);
            }

            return Lengths;
        }

        public double LeftLength()
        {

            List<double> Lengths = new List<double>();
            for (int i = 0; i < parkings.Count; i++)
            {
                int j = (i + 1) % parkings.Count;
                double roadi = parkings[i].necessaryRoad;
                double roadj = parkings[j].necessaryRoad;
                double roadLonger = roadi > roadj ? roadi : roadj;
                Lengths.Add(parkings[i].height);
                Lengths.Add(roadLonger);
            }

            return totalLength - Lengths.Sum();
        }

        public List<int> ParkingUnitCount()
        {
            List<int> Counts = new List<int>();
            for (int i = 0; i < parkings.Count; i++)
            {
                double totalWidth = baseCurve.GetLength();
                int count = 0;
                while (true)
                {
                    double offset = i == 0 ? parkings[i].width : parkings[i].widthOffset;
                    totalWidth -= offset;
                    if (totalWidth > 0)
                    {
                        if (parkings[i].isDouble)
                            count += 2;
                        else
                            count++;
                    }
                    else
                    {
                        Counts.Add(count);
                        break;
                    }
                }
            }

            return Counts;
        }
    }
}
