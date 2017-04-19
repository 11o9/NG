using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
namespace NG.DataStructure
{
    interface ILine
    {
        Point3d Start { get; set; }
        Point3d End { get; set; }
    }

    

    interface IControlLineCollection
    {
        IEnumerator<IControlLine> lines { get; set; }
    }

    interface IControlLine
    {

    }

    interface IProject
    {

    }
    class DataStructure
    {

    }
}
