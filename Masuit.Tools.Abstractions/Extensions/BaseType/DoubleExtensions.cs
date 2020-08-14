namespace Masuit.Tools
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// 将小数截断为8位
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double Digits8(this double num)
        {
            return (long)(num * 1E+8) * 1e-8;
        }
    }
}