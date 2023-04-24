using ZBase.Foundation.FixedArrays;

namespace FixedArrayTests
{
    [FixedArraySize(8)]
    public partial struct FixedArrayInt : IFixedArray<int> { }
}