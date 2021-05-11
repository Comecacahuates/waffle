using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Waffle.Components
{
    /// <summary>
    /// Componente que genera una estructura de waffle radial de una superficie/polisuperficie cerrada.
    /// </summary>
    public class RadialWaffleComponent : GH_Component
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public RadialWaffleComponent()
          : base("Waffle radial", "RadWfl",
              "Estructura de waffle radial de una suérficie/polisuperficie cerrada.",
              "Intersect", "Shape")
        {
            string lang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            if (lang != "es")
                this.Description = "Radial waffle structure from closed brep.";
        }

        /// <summary>
        /// Registrar parámetros de entrada.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            string lang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            if (lang == "es")
            {
                pManager.AddBrepParameter("Brep", "B", "Superficie/polisuperficie cerrada a rebanar.", GH_ParamAccess.item);
                pManager.AddNumberParameter("Distancia vertical", "D", "Distancia vertical entre las rebanadas horizontales.", GH_ParamAccess.item);
                pManager.AddIntegerParameter("Rebanadas radiales", "N", "Número de rebanadas radiales.", GH_ParamAccess.item, 10);
                pManager.AddPointParameter("Centro", "C", "Centro del waffle.", GH_ParamAccess.item, Point3d.Unset);
                pManager.AddNumberParameter("Radio central", "R", "Radio central.", GH_ParamAccess.item, 0.0);
                pManager.AddNumberParameter("Espesor", "T", "Espesor del material.", GH_ParamAccess.item);
                pManager.AddBooleanParameter("Con agujeros", "H", "¿Rebanadas horizontales con agujeros?", GH_ParamAccess.item, true);
            }
            else
            {
                pManager.AddBrepParameter("Brep", "B", lang + " Brep to be sliced.", GH_ParamAccess.item);
                pManager.AddNumberParameter("Vertical distance", "D", "Vertical distance between horizontal slices.", GH_ParamAccess.item);
                pManager.AddIntegerParameter("Radial slices", "N", "Number of radial slices.", GH_ParamAccess.item, 10);
                pManager.AddPointParameter("Center", "C", "Center of the waffle.", GH_ParamAccess.item, Point3d.Unset);
                pManager.AddNumberParameter("Central radius", "R", "Central radius.", GH_ParamAccess.item, 0.0);
                pManager.AddNumberParameter("Thickness", "T", "Material thickness.", GH_ParamAccess.item);
                pManager.AddBooleanParameter("With holes", "H", "With holes in horizontal slices?", GH_ParamAccess.item, true);
            }
        }

        /// <summary>
        /// Registrar parámetros de salida.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            string lang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            if (lang == "es")
            {
                pManager.AddCurveParameter("Rebanadas horizontales", "H", "Rebanadas horizontales.", GH_ParamAccess.list);
                pManager.AddPlaneParameter("Planos horizontales", "HP", "Planos de orientación para las rebanadas horizontales.", GH_ParamAccess.list);
                pManager.AddCurveParameter("Rebanadas radiales", "R", "Rebanadas radiales.", GH_ParamAccess.list);
                pManager.AddPlaneParameter("Planos radiales", "RP", "Planos de orientación para las rebanadas radiales.", GH_ParamAccess.list);
                pManager.AddBoxParameter("Test", "Test", "Test.", GH_ParamAccess.list);
            }
            else
            {
                pManager.AddCurveParameter("Horizontal slices", "H", "Horizontal slices.", GH_ParamAccess.list);
                pManager.AddPlaneParameter("Horizontal planes", "HP", "Orientation planes for horizontal slices.", GH_ParamAccess.list);
                pManager.AddCurveParameter("Radial slices", "R", "Radial slices.", GH_ParamAccess.list);
                pManager.AddPlaneParameter("Radial planes", "RP", "Orientation planes for radial slices.", GH_ParamAccess.list);
                pManager.AddBoxParameter("Test", "Test", "Test.", GH_ParamAccess.list);
            }
        }

        /// <summary>
        /// Crear la estructura de waffle.
        /// </summary>
        /// <param name="DA">Acceso a los parámetros de entrada y salida.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string lang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            Brep brep = new Brep();         // Superifice/polisuperficie cerrada base.
            double slcsDistVert = 0.0;      // Distancie vertical entre rebanadas hroizontales.
            int numSlcsRad = 0;             // Número de rebanadas radiales.
            Point3d cen = Point3d.Unset;    // Centro del waffle.
            double cenRad = 0.0;            // Radio central.
            double thkns = 0.0;             // Espesor del material.
            bool holes = true;              // Agujeros en las rebanadas horizontales.
            /*
             * Se obtienen los parámetros de entrada.
             */
            if (!DA.GetData(0, ref brep))
                return;
            if (!DA.GetData(1, ref slcsDistVert))
                return;
            if (!DA.GetData(2, ref numSlcsRad))
                return;
            if (!DA.GetData(3, ref cen))
                return;
            if (!DA.GetData(4, ref cenRad))
                return;
            if (!DA.GetData(5, ref thkns))
                return;
            if (!DA.GetData(6, ref holes))
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
            if (numSlcsRad <= 0)
            {
                if (lang == "es")
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "El número de rebanadas radiales debe ser mayor que 0.");
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of slices must be greater than 0.");
            }
            if (thkns <= 0)
            {
                if (lang == "es")
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "El espesor del material debe ser mayor que 0.");
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Material thickness must be greater than 0.");
                return;
            }
            if (slcsDistVert <= thkns)
            {
                if (lang == "es")
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "El espesor del material no puede ser más grande que la distancia entre las rebanadas horizontales.");
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Material thickness can't be greater than distance between horizontal slices.");
                return;
            }
            /*
             * Se obtienen las dimensiones.
             */
            BoundingBox bbox = brep.GetBoundingBox(false);
            Point3d pO = bbox.Corner(true, true, true);
            Point3d pX = bbox.Corner(false, true, true);
            Point3d pY = bbox.Corner(true, false, true);
            Point3d pZ = bbox.Corner(true, true, false);
            double minX = pO.X;    // Coordenada X mínima.
            double maxX = pX.X;    // Coordenada X máxima.
            double minY = pO.Y;    // Coordenada Y mínima.
            double maxY = pY.Y;    // Coordenada Y máxima.
            double minZ = pO.Z;    // Coordenada Z mínima.
            double maxZ = pZ.Z;    // Coordenada Z máxima.
            double dX = maxX - minX;    // Dimensión en X.
            double dY = maxY - minY;    // Dimensión en Y.
            double dZ = maxZ - minZ;    // Dimensión en Z.
            double midX = (maxX + minX) / 2.0;    // Coordenada X media.
            double midY = (maxY + minY) / 2.0;    // Coordenada Y media.
            double midZ = (maxZ + minZ) / 2.0;    // Coordenada Z media.
            /*
             * Se pone el centro en la coordenada Z media.
             */
            if (cen == Point3d.Unset)
                cen = new Point3d(midX, midY, midZ);
            else
                cen = new Point3d(cen.X, cen.Y, midZ);
            /*
             * Se generan las rebanadas horizontales.
             */
            int numDivsVert = (int)Math.Floor(dZ / slcsDistVert);
            minZ += (dZ - numDivsVert * slcsDistVert) / 2.0;
            Plane[] plnsHor = new Plane[numDivsVert + 1];    // Planos para las rebanadas horizontales.
            for (int i = 0; i < plnsHor.Length; i++)
            {
                plnsHor[i] = Plane.WorldXY;
                plnsHor[i].Origin = new Point3d(0, 0, minZ + slcsDistVert * i);
            }
            List<Curve> slcsHor = new List<Curve>();    // Rebanadas horizontales.
            foreach (Plane plnHor in plnsHor)
            {
                /*
                 * Se busca la curva y se agrega a la lista de rebanadas horizontales.
                 */
                Curve[] slcs = Brep.CreateContourCurves(brep, plnHor);
                double maxLength = 0.0;
                int maxLengthIndex = 0;
                for (int i = 0; i < slcs.Length; i++)
                    if (maxLength < slcs[i].GetLength())
                    {
                        maxLength = slcs[i].GetLength();
                        maxLengthIndex = i;
                    }
                try
                {
                slcsHor.Add(slcs[maxLengthIndex]);
                } catch(IndexOutOfRangeException)
                {
                    if (lang == "es")
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Intenta con otro valor para la distancia vertical entre rebanadas horizontales.");
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Try a different value for vertical distance between horizontal slices.");
                    return;
                }
            }
            /*
             * Se crean las rebanadas radiales.
             */
            Plane[] plnsRad = new Plane[numSlcsRad];    // Planos para las rebanadas radiales.
            plnsRad[0] = new Plane(cen, Vector3d.XAxis, Vector3d.ZAxis);
            double angle = 2 * Math.PI / numSlcsRad;
            for (int i = 1; i < plnsRad.Length; i++)
            {
                plnsRad[i] = plnsRad[i - 1].Clone();
                plnsRad[i].Rotate(angle, plnsRad[i].YAxis);
            }
            List<Curve> slcsRad = new List<Curve>();    // Rebanadas radiales.
            double tolerance = 0.001;
            foreach (Plane plnRad in plnsRad)
            {
                /*
                 * Se busca la curva y se agrega a la lista de rebanadas horizontales.
                 */
                Curve[] slcs = Brep.CreateContourCurves(brep, plnRad);
                double maxLength = 0.0;
                int maxLengthIndex = 0;
                for (int i = 0; i < slcs.Length; i++)
                    if (maxLength < slcs[i].GetLength())
                    {
                        maxLength = slcs[i].GetLength();
                        maxLengthIndex = i;
                    }
                Curve slc = slcs[maxLengthIndex];
                /*
                 * Se crea el rectángulo para recortar la rebanada.
                 */
                BoundingBox bboxRad = slc.GetBoundingBox(plnRad);
                Point3d pORad = bbox.Corner(true, true, true);
                Point3d pXRad = bbox.Corner(false, true, true);
                double minXRad = pORad.X;    // Coordenada X mínima.
                double maxXRad = pXRad.X;    // Coordenada X máxima.
                double dXRad = maxXRad - minXRad;    // Dimensión en X.
                Interval intvlX = new Interval(-dXRad, cenRad);
                Interval intvlY = new Interval(-dZ, dZ);
                Rectangle3d rectCen = new Rectangle3d(plnRad, intvlX, intvlY);    // Rectángulo de recorte.
                /*
                 * Se hace la diferencia booleana con el rectángulo.
                 */
                Curve[] boolDiffCrvs = Curve.CreateBooleanDifference(slc, rectCen.ToNurbsCurve(), tolerance);
                slcsRad.AddRange(boolDiffCrvs);
            }
            /*
             * Se obtienen las intersecciones de los planos horizontales con las rebanadas radiales.
             */
            DataTree<Line> grvsLnRad = new DataTree<Line>();    // Líneas de intersección para las rebanadas radiales.
            DataTree<Line> grvsLnHor = new DataTree<Line>();    // Líneas de intersección para las rebanadas horizontales.
            for (int i = 0; i < slcsRad.Count; i++)
            {
                Curve slc = slcsRad[i];
                for (int j = 0; j < plnsHor.Length; j++)
                {
                    Plane pln = plnsHor[j];
                    CurveIntersections cpxs = Intersection.CurvePlane(slc, pln, tolerance);
                    if (cpxs.Count == 2)
                    {
                        IntersectionEvent cpx0 = cpxs[0];
                        IntersectionEvent cpx1 = cpxs[1];
                        Line line = new Line(cpx0.PointA, cpx1.PointA);
                        grvsLnRad.Add(line, new GH_Path(i));
                        grvsLnHor.Add(line, new GH_Path(j));
                    }
                }
            }
            /*
             * Se crean las ranuras en las rebanadas radiales y los planos de orientación.
             */
            List<BoundingBox> test = new List<BoundingBox>();
            for (int i = 0; i < slcsRad.Count; i++)
            {
                List<Line> grvsLnRadBranch = grvsLnRad.Branch(new GH_Path(i));
                Curve slc = slcsRad[i];
                Curve[] grvsRad = new Curve[grvsLnRadBranch.Count + 1];
                Plane pln = plnsRad[i];
                for (int j = 0; j< grvsLnRadBranch.Count; j++)
                {
                    /*
                     * Se crea el rectángulo de la ranura.
                     */
                    Line grvLn = grvsLnRadBranch[j];
                    pln.Origin = grvLn.PointAt(0.5);
                    Interval intvlX = new Interval(-grvLn.Length, 0);
                    Interval intvlY = new Interval(-thkns / 2.0, thkns / 2.0);
                    Rectangle3d rectGrv = new Rectangle3d(pln, intvlX, intvlY);    // Rectángulo de la ranura.
                    grvsRad[j] = rectGrv.ToNurbsCurve();
                }
                /*
                 * Se Calcula la direrencia entre la rebanada y las ranuras.
                 */
                Curve[] boolDiffCrvs = Curve.CreateBooleanDifference(slc, grvsRad, tolerance);
                slcsRad[i] = getLongestCurve(boolDiffCrvs);
                /*
                 * Se obtiene el plano de referencia de la curva.
                 */
                //plnsRad[i] = getCurveReferencePlane(slcsRad[i], plnsRad[i]);
                test.Add(slcsRad[i].GetBoundingBox(plnsRad[i]));
            }
            /*
             * 
             */

            /*
             * Se asignan los valores a los parámetros de salida.
             */
            DA.SetDataList(0, slcsHor);
            DA.SetDataList(1, plnsHor);
            DA.SetDataList(2, slcsRad);
            DA.SetDataList(3, plnsRad);
            DA.SetDataList(4, test);
        }

        /// <summary>
        /// Recibe una lista de curvar y devuelve la cuva cuya longitud es mayor.
        /// </summary>
        /// <param name="crvs">Lista de curvas.</param>
        /// <returns>Curva de longitud mayor.</returns>
        Curve getLongestCurve(IEnumerable<Curve> crvs)
        {
            Curve crvMain = null;
            double maxLength = 0.0;
            foreach (Curve crv in crvs)
            {
                double crvLength = crv.GetLength();
                if (maxLength < crvLength)
                {
                    maxLength = crvLength;
                    crvMain = crv;
                }
            }
            return crvMain;
        }

        Plane getCurveReferencePlane(Curve crv, Plane pln)
        {
            BoundingBox bbox = crv.GetBoundingBox(pln);
            pln.Origin = bbox.Center;
            return pln;
        }

        public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.WaffleComponent_24x24;
        public override Guid ComponentGuid => new Guid("47ba0ac6-0d49-4a4c-9d23-f1f9ecf86e97");
    }
}
