using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Waffle
{
    /// <summary>
    /// Componente que genera una estructura de Waffle de un modelo 3D.
    /// </summary>
    public class WaffleComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public WaffleComponent()
          : base("Waffle", "Wffl",
              "Waffle de objeto 3D.",
              "Intersect", "Shape")
        {
        }

        /// <summary>
        /// Registrar parámetros de entrada.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Objeto a rebanar.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distancia", "D", "Distancia entre las rebanadas.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Grosor", "T", "Grosor del material", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registrar parámetros de salida.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Rebanadas X", "X", "Rebanadas en la dirección de X.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Planos YZ", "PYZ", "Planos para las rebanadas en la dirección X.", GH_ParamAccess.list);
            pManager.AddCurveParameter("Rebanadas Y", "Y", "Rebanadas en la dirección de Y.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Planos XZ", "PXZ", "Planos para las rebanadas en la dirección Y.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = new Brep();
            double distance = 1;
            double thickness = 1;
            /*
             * Se obtienen los parámetros de entrada.
             */
            if (!DA.GetData(0, ref brep))
                return;
            if (!DA.GetData(1, ref distance))
                return;
            if (!DA.GetData(2, ref thickness))
                return;
            /*
             * Se validan los parámetros de entrada.
             */
            if (!brep.IsSolid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "El brep debe ser sólido.");
                return;
            }
            if (distance <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "La distancia de separación de las rebanadas debe ser mayor que 0.");
            }
            if (thickness <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "El grosor del material debe ser mayor que 0.");
                return;
            }
            /*
             * Se obtienen las dimensiones.
             */
            BoundingBox bbox = brep.GetBoundingBox(false);
            Point3d pO = bbox.Corner(true, true, true);
            Point3d pX = bbox.Corner(false, true, true);
            Point3d pY = bbox.Corner(true, false, true);
            double minX = pO.X;    // Coordenada X mínima.
            double maxX = pX.X;    // Coordenada X máxima.
            double minY = pO.Y;    // Coordenada Y mínima.
            double maxY = pY.Y;    // Coordenada Y máxima.
            double dX = maxX - minX;    // Dimensión en X.
            double dY = maxY - minY;    // Dimensión en Y.
            double midX = minX + dX / 2.0;    // Coordenada X media.
            double midY = minY + dY / 2.0;    // Coordenada Y media.
            /*
             * Se obtienen las rebanadas en X.
             */
            int numDivX = (int)Math.Floor(dX / distance);
            minX += (dX - numDivX * distance) / 2.0;
            maxX = minX + numDivX * distance;
            Plane[] plnsX = new Plane[numDivX + 1];
            for (int i = 0; i < plnsX.Length; i++)
            {
                Point3d p = new Point3d(minX + distance * i, 0, 0);
                plnsX[i] = new Plane(p, Vector3d.XAxis);
            }
            List<Curve> slcsX = new List<Curve>(); // Rebanadas en X.
            foreach (Plane pln in plnsX)
                slcsX.AddRange(Brep.CreateContourCurves(brep, pln));
            /*
             * Se obtienen las rebanadas en Y.
             */
            int numDivY = (int)Math.Floor(dY / distance);
            minY += (dY - numDivY * distance) / 2.0;
            maxY = minY + numDivY * distance;
            Plane[] plnsY = new Plane[numDivY + 1];
            for (int i = 0; i < plnsY.Length; i++)
            {
                Point3d p = new Point3d(0, minY + distance * i, 0);
                plnsY[i] = new Plane(p, -Vector3d.YAxis);
            }
            List<Curve> slcsY = new List<Curve>();    // Rebanadas en Y.
            foreach (Plane pln in plnsY)
                slcsY.AddRange(Brep.CreateContourCurves(brep, pln));
            /*
             * Se calculan las intersecciones entre las rebanadas en X y las rebanadas en Y.
             */
            List<Line>[] grvsLnX = new List<Line>[slcsX.Count];    // Líneas para las intersecciones de las curvas en X.
            for (int i = 0; i < grvsLnX.Length; i++)
                grvsLnX[i] = new List<Line>();
            List<Line>[] grvsLnY = new List<Line>[slcsY.Count];    // Líneas para las intersecciones de las curvas en Y.
            for (int i = 0; i < grvsLnY.Length; i++)
                grvsLnY[i] = new List<Line>();
            const double intersection_tolerance = 0.001;
            const double overlap_tolerance = 0.0;
            for (int i = 0; i < slcsX.Count; i++)
            {
                Curve crvX = slcsX[i];
                for (int j = 0; j < slcsY.Count; j++)
                {
                    Curve crvY = slcsY[j];
                    CurveIntersections ccxs = Intersection.CurveCurve(crvX, crvY, intersection_tolerance, overlap_tolerance);
                    /*
                     * Únicamente se consideran los pares de curvas que tengan dos puntos de intersección.
                     */
                    if (ccxs.Count == 2)
                    {
                        IntersectionEvent ccx0 = ccxs[0];
                        IntersectionEvent ccx1 = ccxs[1];
                        Line line = new Line(ccx0.PointA, ccx1.PointA);
                        grvsLnX[i].Add(line);
                        grvsLnY[j].Add(line);
                    }


                }
            }
            /*
             * Se crean las las ranuras de las rebanadas en X y los planos de orientación.
             */
            Curve[] crvsX = new Curve[slcsX.Count];
            Plane[] plnsYZ = new Plane[slcsX.Count];
            double tolerance = 0.001;
            for (int i = 0; i < slcsX.Count; i++)
            {
                List<Curve> grvsX = new List<Curve>();
                foreach (Line grvLn in grvsLnX[i])
                {
                    Point3d o = grvLn.PointAt(0.5);
                    Plane pln = new Plane(o, Vector3d.YAxis, Vector3d.ZAxis);
                    Interval intvlX = new Interval(-thickness / 2.0, thickness / 2.0);
                    Interval intvlY = new Interval(0, grvLn.Length);
                    Rectangle3d grv = new Rectangle3d(pln, intvlX, intvlY);
                    grvsX.Add(grv.ToNurbsCurve());
                }
                /*
                 * Se calcula la diferencia de la rebanada y las ranuras.
                 */
                Plane plnYZ;
                slcsX[i].TryGetPlane(out plnYZ, tolerance);
                grvsX.Add(slcsX[i]);
                CurveBooleanRegions crvBoolReg = Curve.CreateBooleanRegions(grvsX, plnYZ, false, tolerance);
                double maxLength = 0.0;
                for (int j = 0; j < crvBoolReg.RegionCount; j++)
                    if (maxLength < crvBoolReg.RegionCurves(j)[0].GetLength())
                    {
                        maxLength = crvBoolReg.RegionCurves(j)[0].GetLength();
                        crvsX[i] = crvBoolReg.RegionCurves(j)[0];
                    }
                /*
                 * Se obtiene el plano de orientación de la rebanada.
                 */
                BoundingBox bboxX = crvsX[i].GetBoundingBox(false);
                plnYZ = Plane.WorldYZ;
                plnYZ.Origin = bboxX.Center;
                plnsYZ[i] = plnYZ;
            }
            /*
             * Se crean las ranuras de las rebanadas en X.
             */
            Curve[] crvsY = new Curve[slcsY.Count];
            Plane[] plnsXZ = new Plane[slcsX.Count];
            for (int i = 0; i < slcsY.Count; i++)
            {
                List<Curve> grvsY = new List<Curve>();
                foreach (Line grvLn in grvsLnY[i])
                {
                    Point3d o = grvLn.PointAt(0.5);
                    Plane pln = new Plane(o, Vector3d.XAxis, -Vector3d.ZAxis);
                    Interval intvlX = new Interval(-thickness / 2.0, thickness / 2.0);
                    Interval intvlY = new Interval(0, grvLn.Length);
                    Rectangle3d grv = new Rectangle3d(pln, intvlX, intvlY);
                    grvsY.Add(grv.ToNurbsCurve());
                }
                /*
                 * Se utiliza el método `CreateBooleanRegions` porque el método `CreateBooleanDifference`.
                 * TODO: Ver si se puede hacer la diferencia booleana de otra manera.
                 */
                Plane plnXZ;
                slcsY[i].TryGetPlane(out plnXZ, tolerance);
                grvsY.Add(slcsY[i]);
                CurveBooleanRegions crvBoolReg = Curve.CreateBooleanRegions(grvsY, plnXZ, false, tolerance);
                double maxLength = 0.0;
                for (int j = 0; j < crvBoolReg.RegionCount; j++)
                    if (maxLength < crvBoolReg.RegionCurves(j)[0].GetLength())
                    {
                        maxLength = crvBoolReg.RegionCurves(j)[0].GetLength();
                        crvsY[i] = crvBoolReg.RegionCurves(j)[0];
                    }
                /*
                 * Se obtiene el plano de orientación de la rebanada.
                 */
                BoundingBox bboxY = crvsY[i].GetBoundingBox(false);
                plnXZ = Plane.WorldZX;
                plnXZ.Origin = bboxY.Center;
                plnsXZ[i] = plnXZ;
            }
            /*
             * Se asignan los valores a los parámetros de salida.
             */
            DA.SetDataList(0, crvsX);
            DA.SetDataList(1, plnsYZ);
            DA.SetDataList(2, crvsY);
            DA.SetDataList(3, plnsXZ);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1e3c76b8-023d-4760-9c3f-97c2665c544a"); }
        }
    }
}
