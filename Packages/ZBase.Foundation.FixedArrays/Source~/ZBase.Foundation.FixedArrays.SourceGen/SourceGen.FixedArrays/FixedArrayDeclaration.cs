using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ZBase.Foundation.FixedArrays
{
    public partial class FixedArrayDeclaration
    {
        public StructDeclarationSyntax Syntax { get; set; }

        public int ArraySize { get; set; }

        public TypeInfo InterfaceTypeInfo { get; set; }

        public TypeInfo ElementTypeInfo { get; set; }
    }
}
