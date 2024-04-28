using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class HTri
    {
        /// Currently Triangle .... Change to Quad Please haha

        public HVertex v1;
        public HVertex v2;
        public HVertex v3;

        public HEdge hEdge;

        // Constructor
        public HTri(HVertex v1, HVertex v2, HVertex v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public HTri(Vec3d v1, Vec3d v2, Vec3d v3)
        {
            this.v1 = new HVertex(v1);
            this.v2 = new HVertex(v2);
            this.v3 = new HVertex(v3);
        }

        public HTri(HEdge halfEdge)
        {
            this.hEdge = halfEdge;
        }


        // Methods

        // Change orientation of triangle from clockwise to counterclockwise or the other way around
        // 
        public void ChangeOrientation()
        {
            HVertex tempV = this.v1;
            this.v1 = this.v2;
            this.v2 = tempV;
        }

    }
}
