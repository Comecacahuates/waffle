using Grasshopper;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waffle.Geometry
{
    /// <summary>
    /// Clase que calcula una estructura de waffle ortogonal.
    /// </summary>
    public class OrthogonalWaffle : WaffleBase
    {
        /// <summary>
        /// Distancia entre las rebanadas.
        /// </summary>
        protected double _distanceBetweenSlices;
        /// <summary>
        /// Rebanadas a lo largo del eje X.
        /// </summary>
        protected List<Curve> _slicesX;
        public List<Curve> SlicesX
        {
            get { return _slicesX; }
        }
        /// <summary>
        /// Rebanadas a lo largo del eje Y.
        /// </summary>
        protected List<Curve> _slicesY;
        public List<Curve> SlicesY
        {
            get { return _slicesY; }
        }
        /// <summary>
        /// Planos de referencia para las rebanadas a lo largo del eje X.
        /// </summary>
        protected List<Plane> _planesX;
        public List<Plane> PlanesX
        {
            get { return _planesX; }
        }
        /// <summary>
        /// Planos de referencia para las rebanadas a lo largo del eje Y.
        /// </summary>
        protected List<Plane> _planesY;
        public List<Plane> PlanesY
        {
            get { return _planesY; }
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseBrep">Superficie/polisuperficie base.</param>
        /// <param name="cuttingMaterialThickness">Espesor del material de corte.</param>
        /// <param name="distanceBetweenSlices"></param>
        public OrthogonalWaffle(Brep baseBrep, double cuttingMaterialThickness, double distanceBetweenSlices) : base(baseBrep, cuttingMaterialThickness)
        {
            _distanceBetweenSlices = distanceBetweenSlices;
        }

        public void computeWaffle()
        {
            /*
             * Se obtienen los planos de corte en X y Y.
             */
            IEnumerable<Plane> slicingPlanesX = getSlicingPlanesX(_distanceBetweenSlices);
            IEnumerable<Plane> slicingPlanesY = getSlicingPlanesY(_distanceBetweenSlices);
            /*
             * Se calculan las rebanadas en X y las intersecciones de estas con los planos de corte en Y.
             */
            DataTree<Curve> slicesX = sliceBaseBrep(slicingPlanesX);

            /*
             * Se obtienen las rebanadas en el eje Y.
             */
            DataTree<Curve> slicesY = sliceBaseBrep(slicingPlanesY);
        }
    }
}
