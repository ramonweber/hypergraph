using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    internal class RPlane
    {
        public Vec3d position;
        public Vec3d normal;

        public RPlane(Vec3d pos, Vec3d normal)
        {
            this.position = pos;
            this.normal = normal;
        }
    }
}
