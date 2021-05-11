using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waffle.Geometry
{
    /// <summary>
    /// Clase que representa una estructura de waffle.
    /// </summary>
    public abstract class WaffleBase
    {
        /// <summary>
        /// Superficie/polisuperficie base.
        /// </summary>
        protected Brep _baseBrep;
        /// <summary>
        /// Caja delimitadora de la superficie/polisuperficie base.
        /// </summary>
        protected BoundingBox _bbox;
        /// <summary>
        /// Espesor del material de corte.
        /// </summary>
        protected double _cuttingMaterialThickness;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseBrep">Superficie/polisuperficie base.</param>
        /// <param name="cuttingMaterialThickness">Espesor del material de corte.</param>
        protected WaffleBase(Brep baseBrep, double cuttingMaterialThickness)
        {
            _baseBrep = baseBrep;
            _cuttingMaterialThickness = cuttingMaterialThickness;
            _bbox = _baseBrep.GetBoundingBox(false);
        }

        /// <summary>
        /// Obtener planos de corte a lo largo del eje X.
        /// </summary>
        /// <param name="distanceBetweenSlices">Distancia entre las rebanadas.</param>
        /// <returns></returns>
        public IEnumerable<Plane> getSlicingPlanesX(double distanceBetweenSlices)
        {
            Point3d pO = _bbox.Corner(true, true, true);
            Point3d pX = _bbox.Corner(false, true, true);
            double minX = pO.X;    // Coordenada X mínima.
            double maxX = pX.X;    // Coordenada X máxima.
            double dX = maxX - minX;    // Dimensión en X.
            int numDivisions = (int)Math.Floor(dX / distanceBetweenSlices);    // Número de divisiones.
            minX += (dX - numDivisions * distanceBetweenSlices) / 2.0;
            Plane[] planes = new Plane[numDivisions + 1];
            for (int i = 0; i < planes.Length; i++)
            {
                planes[i] = Plane.WorldYZ;
                planes[i].Origin = new Point3d(minX + distanceBetweenSlices * i, 0, 0);
            }
            return planes;
        }

        /// <summary>
        /// Obtener planos de corte a lo largo del eje Y.
        /// </summary>
        /// <param name="distanceBetweenSlices">Distancia entre las rebanadas.</param>
        /// <returns></returns>
        public IEnumerable<Plane> getSlicingPlanesY(double distanceBetweenSlices)
        {
            Point3d pO = _bbox.Corner(true, true, true);
            Point3d pY = _bbox.Corner(true, false, true);
            double minY = pO.Y;    // Coordenada Y mínima.
            double maxY = pY.Y;    // Coordenada Y máxima.
            double dY = maxY - minY;    // Dimensión en Y.
            int numDivisions = (int)Math.Floor(dY / distanceBetweenSlices);    // Número de divisiones.
            minY += (dY - numDivisions * distanceBetweenSlices) / 2.0;
            Plane[] planes = new Plane[numDivisions + 1];
            for (int i = 0; i < planes.Length; i++)
            {
                planes[i] = Plane.WorldYZ;
                planes[i].Origin = new Point3d(0, minY + distanceBetweenSlices * i, 0);
            }
            return planes;
        }

        /// <summary>
        /// Obtener planos de corte a lo largo del eje Z.
        /// </summary>
        /// <param name="distanceBetweenSlices">Distancia entre las rebanadas.</param>
        /// <returns></returns>
        public IEnumerable<Plane> getSlicingPlanesZ(double distanceBetweenSlices)
        {
            Point3d pO = _bbox.Corner(true, true, true);
            Point3d pZ = _bbox.Corner(true, true, false);
            double minZ = pO.Z;    // Coordenada Z mínima.
            double maxZ = pZ.Z;    // Coordenada Z máxima.
            double dZ = maxZ - minZ;    // Dimensión en Z.
            int numDivisions = (int)Math.Floor(dZ / distanceBetweenSlices);    // Número de divisiones.
            minZ += (dZ - numDivisions * distanceBetweenSlices) / 2.0;
            Plane[] planes = new Plane[numDivisions + 1];
            for (int i = 0; i < planes.Length; i++)
            {
                planes[i] = Plane.WorldYZ;
                planes[i].Origin = new Point3d(0, minZ + distanceBetweenSlices * i, 0);
            }
            return planes;
        }

        /// <summary>
        /// Calcula las rebanadas con los planos de corte.
        /// </summary>
        /// <param name="planes"></param>
        /// <returns></returns>
        public DataTree<Curve> sliceBaseBrep(IEnumerable<Plane> planes)
        {
            DataTree<Curve> slices = new DataTree<Curve>();
            int i = 0;
            foreach (Plane plane in planes)
                slices.AddRange(Brep.CreateContourCurves(_baseBrep, plane), new GH_Path(i++));
            return slices;
        }
    }
}
