using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;

namespace Waffle
{
    /// <summary>
    /// Componente que genera una estructura de Waffle de una superficie/polisuperficie cerrada.
    /// </summary>
    public class WaffleComponent : GH_Component
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public WaffleComponent()
          : base("Waffle", "Wfl",
              "Estructura de waffle de una superficie/polisuperficie cerrada.",
              "Intersect", "Shape")
        {
            string lang = System.Globalization.CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            if (lang != "es")
                this.Description = "Waffle structure from closed brep.";
        }

        /// <summary>
        /// Registrar parámetros de entrada.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            string lang = System.Globalization.CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            if (lang == "es")
            {
                pManager.AddBrepParameter("Brep", "B", "Superficie/polisuperficie cerrada a rebanar.", GH_ParamAccess.item);
                pManager.AddNumberParameter("Distancia", "D", "Distancia entre las rebanadas.", GH_ParamAccess.item);
                pManager.AddNumberParameter("Espesor", "T", "Espesor del material.", GH_ParamAccess.item);
            }
            else
            {
                pManager.AddBrepParameter("Brep", "B", lang + " Brep to be sliced.", GH_ParamAccess.item);
                pManager.AddNumberParameter("Distance", "D", "Distance between slices.", GH_ParamAccess.item);
                pManager.AddNumberParameter("Thickness", "T", "Material thickness.", GH_ParamAccess.item);
            }
        }

        /// <summary>
        /// Registrar parámetros de salida.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            string lang = System.Globalization.CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            if (lang == "es")
            {
                pManager.AddCurveParameter("Rebanadas X", "X", "Rebanadas en la dirección de X.", GH_ParamAccess.list);
                pManager.AddPlaneParameter("Planos YZ", "PYZ", "Planos de orientación para las rebanadas en la dirección X.", GH_ParamAccess.list);
                pManager.AddCurveParameter("Rebanadas Y", "Y", "Rebanadas en la dirección de Y.", GH_ParamAccess.list);
                pManager.AddPlaneParameter("Planos XZ", "PXZ", "Planos de orientación para las rebanadas en la dirección Y.", GH_ParamAccess.list);
            }
            else
            {
                pManager.AddCurveParameter("X slices", "X", "Slices in X direction.", GH_ParamAccess.list);
                pManager.AddPlaneParameter("YZ planes", "PYZ", "Orientation planes for slices in X direction.", GH_ParamAccess.list);
                pManager.AddCurveParameter("Y slices", "Y", "Slices in Y direction.", GH_ParamAccess.list);
                pManager.AddPlaneParameter("XZ planes", "PXZ", "Orientation planes for slices in Y direction.", GH_ParamAccess.list);
            }
        }

        /// <summary>
        /// Crear la estructura de waffle.
        /// </summary>
        /// <param name="DA">Acceso a los parámetros de entrada y salida.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string lang = System.Globalization.CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            Brep brep = new Brep();
            double slcDist = 0;
            double thkns = 0;
            /*
             * Se obtienen los parámetros de entrada.
             */
            if (!DA.GetData(0, ref brep))
                return;
            if (!DA.GetData(1, ref slcDist))
                return;
            if (!DA.GetData(2, ref thkns))
                return;
            /*
             * Se validan los parámetros de entrada.
             */
            if (!brep.IsSolid)
            {
                if (lang == "es")
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "La superficie/polisuperficie debe ser sólido.");
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Brep must be closed.");
                return;
            }
            if (slcDist <= 0)
            {
                if (lang == "es")
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "La distancia entre las rebanadas debe ser mayor que 0.");
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Distance between slices must be grater than 0.");
                return;
            }
            if (thkns <= 0)
            {
                if (lang == "es")
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "El espesor del material debe ser mayor que 0.");
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Material thickness must be greater than 0.");
                return;
            }
            if (slcDist <= thkns)
            {
                if (lang == "es")
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "El espesor del material no puede ser más grande que la distancia entre las rebanadas.");
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Material thickness can't be greater than distance between slices.");
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
             * Se generan las rebanadas en X.
             */
            int numDivX = (int)Math.Floor(dX / slcDist);
            minX += (dX - numDivX * slcDist) / 2.0;
            maxX = minX + numDivX * slcDist;
            Plane[] plnsX = new Plane[numDivX + 1];
            for (int i = 0; i < plnsX.Length; i++)
            {
                Point3d p = new Point3d(minX + slcDist * i, 0, 0);
                plnsX[i] = new Plane(p, Vector3d.XAxis);
            }
            List<Curve> slcsX = new List<Curve>(); // Rebanadas en X.
            foreach (Plane pln in plnsX)
                slcsX.AddRange(Brep.CreateContourCurves(brep, pln));
            /*
             * Se generan las rebanadas en Y.
             */
            int numDivY = (int)Math.Floor(dY / slcDist);
            minY += (dY - numDivY * slcDist) / 2.0;
            maxY = minY + numDivY * slcDist;
            Plane[] plnsY = new Plane[numDivY + 1];
            for (int i = 0; i < plnsY.Length; i++)
            {
                Point3d p = new Point3d(0, minY + slcDist * i, 0);
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
                    Interval intvlX = new Interval(-thkns / 2.0, thkns / 2.0);
                    Interval intvlY = new Interval(0, grvLn.Length);
                    Rectangle3d grv = new Rectangle3d(pln, intvlX, intvlY);
                    grvsX.Add(grv.ToNurbsCurve());
                }
                /*
                 * Se calcula la diferencia entre la rebanada y las ranuras.
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
                try
                {
                    BoundingBox bboxX = crvsX[i].GetBoundingBox(false);
                    plnYZ = Plane.WorldYZ;
                    plnYZ.Origin = bboxX.Center;
                    plnsYZ[i] = plnYZ;
                }
                catch (NullReferenceException)
                {
                    if (lang == "es")
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error.");
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error.");
                    return;
                }
            }
            /*
             * Se crean las ranuras de las rebanadas en Y.
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
                    Interval intvlX = new Interval(-thkns / 2.0, thkns / 2.0);
                    Interval intvlY = new Interval(0, grvLn.Length);
                    Rectangle3d grv = new Rectangle3d(pln, intvlX, intvlY);
                    grvsY.Add(grv.ToNurbsCurve());
                }
                /*
                 * Se utiliza el método `CreateBooleanRegions` porque el método `CreateBooleanDifference`.
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
                plnXZ = new Plane(bboxY.Center, Vector3d.XAxis, -Vector3d.ZAxis);
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

        public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.WaffleComponent_24x24;
        public override Guid ComponentGuid => new Guid("1e3c76b8-023d-4760-9c3f-97c2665c544a");
    }
}
