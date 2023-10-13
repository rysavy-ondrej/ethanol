using System.Net;

namespace Ethanol.ContextBuilder.Helpers
{
    /// <summary>
    /// A static class with methods for safely converting objects to various data types, returning a bool indicating whether the conversion was successful, and an output parameter containing the converted value on success.
    /// </summary>
    public static class TryConvert
    {
        /// <summary>
        /// Attempts to convert the specified object to a ushort. Returns true if the conversion was successful, and sets the result output parameter to the converted value on success.
        /// </summary>
        public static bool ToUInt16(object value, out ushort result)
        {
            if (value == null)
            {
                result = 0;
                return false;
            }

            string strValue = value.ToString();
            return ushort.TryParse(strValue, out result);
        }

        /// <summary>
        /// Attempts to convert the specified object to a uint. Returns true if the conversion was successful, and sets the result output parameter to the converted value on success.
        /// </summary>
        public static bool ToUInt32(object value, out uint result)
        {
            if (value == null)
            {
                result = 0;
                return false;
            }

            string strValue = value.ToString();
            return uint.TryParse(strValue, out result);
        }

        /// <summary>
        /// Attempts to convert the specified object to a float. Returns true if the conversion was successful, and sets the result output parameter to the converted value on success.
        /// </summary>
        public static bool ToFloat(object value, out float result)
        {
            if (value == null)
            {
                result = 0;
                return false;
            }

            string strValue = value.ToString();
            return float.TryParse(strValue, out result);
        }

        /// <summary>
        /// Attempts to convert the specified object to an IPAddress. Returns true if the conversion was successful, and sets the result output parameter to the converted value on success.
        /// </summary>
        public static bool ToIPAddress(object value, out IPAddress result)
        {
            if (value == null)
            {
                result = null;
                return false;
            }

            string strValue = value.ToString();
            return IPAddress.TryParse(strValue, out result);
        }
    }

}
