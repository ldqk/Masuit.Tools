namespace Masuit.Tools.UnitTest.Mapping.ClassTests
{
    public class ClassDest2 : IClassDest2
    {

        public int PropInt1 { get; set; }
        public int PropInt2 { get; set; }
        public string PropString2 { get; set; }

        public ClassSource ClassSourceProp { get; set; }

    }
}
