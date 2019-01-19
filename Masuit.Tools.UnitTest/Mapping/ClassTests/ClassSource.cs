using System.Collections.Generic;

namespace Masuit.Tools.UnitTest.Mapping.ClassTests
{
    public class ClassSource
    {
        public int PropInt1 { get; set; }
        public int PropSourceInt1 { get; set; }
        public string PropString1 { get; set; }

        public string PropString { get; set; }

        public string PropString2 { get; set; }
        public List<ClassSource2> ListProp { get; set; }

        public List<string> ListString { get; set; }
        public ClassSource Same { get; set; }

        public ClassSource2 SubClass { get; set; }
    }
}
