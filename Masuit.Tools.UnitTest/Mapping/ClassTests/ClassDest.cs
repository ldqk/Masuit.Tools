using System.Collections.Generic;

namespace Masuit.Tools.UnitTest.Mapping.ClassTests
{
    public class ClassDest
    {

        public int PropInt1 { get; set; }
        public int PropInt2 { get; set; }

        public string PropString { get; set; }
        public string PropString2 { get; set; }


        public int RealOnlyPropInt1 { get; }

        public ClassDest2 ClassDestEntityBase { get; set; }
        public List<ClassDest2> ListProp { get; set; }

        public List<string> ListString { get; set; }
        public ClassDest2 SubClass { get; set; }
        public ClassDest2 SubClass2 { get; set; }
        public int CountListProp { get; set; }
    }
}
