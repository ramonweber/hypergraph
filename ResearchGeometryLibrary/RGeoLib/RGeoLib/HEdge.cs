using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class HEdge
    {
        // ---------------------------------------------
        // Properties

        // The vertex the edge leads to
        public HVertex v;

        // Face associated with this halfedge
        public HTri triFace;

        // Prev and next edge
        public HEdge prevEdge;
        public HEdge nextEdge;

        // Edge going in opposite direction
        public HEdge oppositeEdge;

        // ---------------------------------------------
        // Constructors
        public HEdge(HVertex v)
        {
            this.v = v;
        }

    }
}
