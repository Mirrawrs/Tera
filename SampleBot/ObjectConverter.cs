using System;
using Lotus.Dispatching.Attributes;

namespace SampleBot
{
    //Used by the Dispatcher to convert strings to other values. The converter methods must be marked by the 
    //Converter attribute, be static, have exactly one parameter and non-void return type.
    internal class ObjectConverter
    {
        [Converter]
        public static int ConvertStringToInt32(string value) => int.Parse(value);

        [Converter]
        public static bool ConvertStringToBoolean(string value)
            => string.Equals(value, true.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}