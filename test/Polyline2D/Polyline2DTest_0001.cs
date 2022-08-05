using Xunit;
using System.Linq;
using System;
using netDxf.Entities;

namespace netDxf.Tests
{
    public partial class Polyline2DTests
    {

        [Fact]
        public void Polyline2DTest_0001()
        {
            var tol = 1e-8;

            var dxf = DxfDocument.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Polyline2D/Polyline2DTest_0001.dxf"));

            var lwp = dxf.Entities.Polylines2D.First();

            var arcs = lwp.Explode().Where(r => r.Type == Entities.EntityType.Arc).Cast<Arc>().ToList();

            arcs[0].StartAngle.AssertEqualsTol(tol, 214.7237306);
            arcs[0].EndAngle.AssertEqualsTol(tol, 210.84103981);

            arcs[1].StartAngle.AssertEqualsTol(tol, 212.93067534);
            arcs[1].EndAngle.AssertEqualsTol(tol, 147.06932466);

            arcs[2].StartAngle.AssertEqualsTol(tol, 149.15896019);
            arcs[2].EndAngle.AssertEqualsTol(tol, 145.2762694);

            arcs[3].StartAngle.AssertEqualsTol(tol, 145.2762694);
            arcs[3].EndAngle.AssertEqualsTol(tol, 214.7237306);
        }

    }
}