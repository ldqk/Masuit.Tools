namespace Masuit.Tools.Mapping
{
    public static class ExpressionCpoier
    {
        public static T Copy<T>(this T source)
        {
            return Copier<T>.Copy(source);
        }
    }
}