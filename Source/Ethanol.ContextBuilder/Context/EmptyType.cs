using System;

namespace Ethanol.ContextBuilder.Context
{
    public struct Empty : IEquatable<Empty>
    {
        //
        // Summary:
        //     The single Empty value.
        public static Empty Default { get; }

        //
        // Summary:
        //     Determines whether the argument Empty value is equal to the receiver. Because
        //     Empty has a single value, this always returns true.
        //
        // Parameters:
        //   other:
        //     An Empty value to compare to the current Empty value.
        //
        // Returns:
        //     Because there is only one value of type Empty, this always returns true.
        public bool Equals(Empty other)
        {
            return true;
        }
        //
        // Summary:
        //     Determines whether the specified System.Object is equal to the current Empty.
        //
        // Parameters:
        //   obj:
        //     The System.Object to compare with the current Empty.
        //
        // Returns:
        //     true if the specified System.Object is a Empty value; otherwise, false.
        public override bool Equals(object obj)
        {
            return true;
        }
        //
        // Summary:
        //     Returns the hash code for the Empty value.
        //
        // Returns:
        //     A hash code for the Empty value.
        public override int GetHashCode()
        {
            return 0;
        }
        //
        // Summary:
        //     Returns a string representation of the Empty value.
        //
        // Returns:
        //     String representation of the Empty value.
        public override string ToString()
        {
            return "";
        }

        //
        // Summary:
        //     Determines whether the two specified Emtpy values are equal. Because Empty has
        //     a single value, this always returns true.
        //
        // Parameters:
        //   first:
        //     The first Empty value to compare.
        //
        //   second:
        //     The second Empty value to compare.
        //
        // Returns:
        //     Because Empty has a single value, this always returns true.
        public static bool operator ==(Empty first, Empty second)
        {
            return true;
        }
        //
        // Summary:
        //     Determines whether the two specified Empty values are not equal. Because Empty
        //     has a single value, this always returns false.
        //
        // Parameters:
        //   first:
        //     The first Empty value to compare.
        //
        //   second:
        //     The second Empty value to compare.
        //
        // Returns:
        //     Because Empty has a single value, this always returns false.
        public static bool operator !=(Empty first, Empty second)
        {
            return false;
        }
    }
}