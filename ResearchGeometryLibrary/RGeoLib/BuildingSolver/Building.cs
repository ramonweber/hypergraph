using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib.BuildingSolver
{
    public class Building
    {

        public NFace bounds { get; set; }
        public NMesh apartmentBounds { get; set; }
        public NMesh cores { get; set; }
        public List<NLine> circLines { get; set; }
        public List<NLine> facadeLines { get; set; }

        public BuildingResult eval { get; set; }

        public Building(NFace bounds, NMesh apartmentBounds, NMesh cores)
        {
            this.bounds = bounds;
            this.apartmentBounds = apartmentBounds;
            this.cores = cores;
        }
    }
}
