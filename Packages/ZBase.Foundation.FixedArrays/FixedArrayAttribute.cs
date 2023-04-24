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
        public int Length { get => default; }
    }

    public interface IFixedArray<T> : IFixedArray
    {
        public T this[int index] { get => default; set => value = default; }

        public Span<T> AsSpan() => default;
    }
}
