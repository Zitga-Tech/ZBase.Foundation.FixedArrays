using System;

namespace ZBase.Foundation.FixedArrays
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class FixedArraySizeAttribute : Attribute
    {
        public int Length { get; private set; }

        public FixedArraySizeAttribute(int length)
        {
            Length = length;
        }
    }

    public interface IFixedArray
    {
        int Length { get; }
    }

    public interface IFixedArray<T> : IFixedArray
    {
        T this[int index] { get; set; }

        Span<T> AsSpan();
    }
}
