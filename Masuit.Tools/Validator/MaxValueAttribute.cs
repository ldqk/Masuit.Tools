using System;
using System.ComponentModel.DataAnnotations;

namespace Masuit.Tools.Core.Validator
{
    /// <summary>
    /// 最大值校验
    /// </summary>
    public class MaxValueAttribute : ValidationAttribute
    {
        private double MaxValue { get; }

        /// <summary>
        /// 最大值
        /// </summary>
        /// <param name="value"></param>
        public MaxValueAttribute(double value)
        {
            MaxValue = value;
        }

        /// <summary>
        /// 最大值校验
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            if (value is null)
            {
                return true;
            }

            var input = Convert.ToDouble(value);
            return input <= MaxValue;
        }

        /// <summary>Applies formatting to an error message, based on the data field where the error occurred.</summary>
        /// <param name="name">The name to include in the formatted message.</param>
        /// <returns>An instance of the formatted error message.</returns>
        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }
}