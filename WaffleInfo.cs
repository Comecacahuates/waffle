using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Waffle
{
    public class WaffleInfo : GH_AssemblyInfo
    {
        public override string Name => "Waffle";
        public override Bitmap Icon => Properties.Resources.WaffleComponent_24x24;
        public override string Description => "Plugin para Grasshopper que genera una estructura de waffle a partir de una superficie/polisuperficie cerrada.";
        public override Guid Id => new Guid("88911191-c82b-44d4-aa51-72212c223843");
        public override string AuthorName => "Adrián Juárez Monroy | Sicadcam";
        public override string AuthorContact => "comecacahuates@yahoo.com";
    }
}
