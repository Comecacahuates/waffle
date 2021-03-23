using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Waffle
{
    /// <summary>
    /// Componente que genera una estructura de Waffle de un modelo 3D.
    /// </summary>
    public class WaffleInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Waffle";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("88911191-c82b-44d4-aa51-72212c223843");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
