using System;
using System.Text;

namespace SharpX.Extensions
{
    public static class ExceptionExtensions
    {
        public static string Format(this Exception exception) => Primitives.FormatException(exception);
    }
}
