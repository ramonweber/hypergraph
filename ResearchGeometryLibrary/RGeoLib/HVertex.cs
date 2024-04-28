using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class HVertex
    {
        // ---------------------------------------------
        // Properties

        public Vec3d position;

        // Half edge that starts at this vector
        public HEdge edge;

        // Face associated with this halfedge
        public HTri triFace;

        // Prev and next vertex
        public HVertex prevVertex;
        public HVertex nextVertex;

        // Properties of Vertex
        public bool isReflex;
        public bool isConvex;
        public bool isEar;


        // ---------------------------------------------
        // Constructors
        public HVertex(Vec3d position)
        {
            this.position = position;
        }

    }
}
