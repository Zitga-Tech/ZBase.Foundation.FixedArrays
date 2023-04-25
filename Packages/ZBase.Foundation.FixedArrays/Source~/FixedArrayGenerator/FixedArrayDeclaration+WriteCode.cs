using ZBase.Foundation.SourceGen;

namespace ZBase.Foundation.FixedArrays
{
    partial class FixedArrayDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"ZBase.Foundation.FixedArrays.FixedArrayGenerator\", \"1.0.2\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        public string WriteCode()
        {
            var elemTypeName = ElementTypeInfo.Type.ToFullName();
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
            p.IncreasedIndent();

            p.PrintLine("[global::System.Runtime.CompilerServices.UnsafeValueType]");
            p.PrintLine("[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 0)]");
            p.PrintLine("[global::System.Diagnostics.CodeAnalysis.SuppressMessage(\"Style\", \"IDE0044:Add readonly modifier\", Justification = \"Required for fixed-size arrays\")]");
            p.PrintBeginLine();

            if (ArraySize > 0)
            {
                p.Print("unsafe ");
            }

            foreach (var m in Syntax.Modifiers)
            {
                p.Print(m.ToString()).Print(" ");
            }

            p.Print("struct ").Print(Syntax.Identifier.Text);

            if (Syntax.TypeParameterList != null
                && Syntax.TypeParameterList.Parameters.Count > 0
            )
            {
                p.Print("<");

                var parameters = Syntax.TypeParameterList.Parameters;
                var last = parameters.Count - 1;

                for (var i = 0; i <= last; i++)
                {
                    var param = parameters[i];
                    p.Print(param.Identifier.ValueText);

                    if (i != last)
                    {
                        p.Print(", ");
                    }
                }

                p.Print(">");
            }

            p.PrintEndLine().IncreasedIndent()
                .PrintLine($": global::System.Collections.Generic.IEnumerable<{elemTypeName}>");

            p.OpenScope();
            {
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public const int SIZE = {ArraySize};");
                p.PrintEndLine();

                for (var i = 0; i < ArraySize; i++)
                {
                    p.PrintLine($"private {elemTypeName} _e{i};");
                }

                if (ArraySize > 0)
                {
                    p.PrintEndLine();
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"public int Length");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => SIZE;");
                }
                p.CloseScope();
                p.PrintEndLine();
                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine().Print($"public {elemTypeName} this[int index]");

                if (ArraySize > 0)
                {
                    p.PrintEndLine();
                    p.OpenScope();
                    {
                        p.PrintLine("get");
                        p.OpenScope();
                        {
                            p.Print("#if ENABLE_UNITY_COLLECTIONS_CHECKS").PrintEndLine();
                            p.PrintLine("if ((uint)index >= (uint)SIZE)");
                            p.OpenScope();
                            p.PrintLine("throw new global::System.IndexOutOfRangeException();");
                            p.CloseScope();
                            p.Print("#endif")
                                .PrintEndLine().PrintEndLine()
                                .PrintLine($"return global::System.Runtime.CompilerServices.Unsafe.Add<{elemTypeName}>(ref _e0, index);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                        p.PrintLine("set");
                        p.OpenScope();
                        {
                            p.Print("#if ENABLE_UNITY_COLLECTIONS_CHECKS").PrintEndLine();
                            p.PrintLine("if ((uint)index >= (uint)SIZE)");
                            p.OpenScope();
                            p.PrintLine("throw new global::System.IndexOutOfRangeException();");
                            p.CloseScope();
                            p.Print("#endif")
                                .PrintEndLine().PrintEndLine()
                                .PrintLine($"ref var elem = ref global::System.Runtime.CompilerServices.Unsafe.Add<{elemTypeName}>(ref _e0, index);")
                                .PrintLine($"elem = value;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                else
                {
                    p.Print($" => throw new global::System.IndexOutOfRangeException();")
                        .PrintEndLine();
                }

                p.PrintEndLine()
                    .PrintLine(AGGRESSIVE_INLINING).PrintLine(EXCLUDE_COVERAGE)
                    .PrintBeginLine()
                    .Print($"public global::System.Span<{elemTypeName}> AsSpan()");

                if (ArraySize > 0)
                {
                    p.PrintEndLine().IncreasedIndent()
                        .PrintLine($"=> new global::System.Span<{elemTypeName}>(global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref _e0), SIZE);");
                }
                else
                {
                    p.Print(" => new();")
                        .PrintEndLine();
                }

                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(EXCLUDE_COVERAGE);

                if (ArraySize > 0)
                {
                    p.PrintLine("public Enumerator GetEnumerator() => new Enumerator(global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref _e0));");
                }
                else
                {
                    p.PrintLine("public Enumerator GetEnumerator() => new Enumerator();");
                }

                p.PrintEndLine()
                    .PrintLine(AGGRESSIVE_INLINING).PrintLine(EXCLUDE_COVERAGE)
                    .PrintLine($"global::System.Collections.Generic.IEnumerator<{elemTypeName}> global::System.Collections.Generic.IEnumerable<{elemTypeName}>.GetEnumerator() => GetEnumerator();");

                p.PrintEndLine()
                    .PrintLine(AGGRESSIVE_INLINING).PrintLine(EXCLUDE_COVERAGE)
                    .PrintLine("global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();");

                p.PrintEndLine();

                if (ArraySize > 0)
                {
                    PrintEnumerator(ref p, elemTypeName);
                }
                else
                {
                    PrintEmptyEnumerator(ref p, elemTypeName);
                }
            }
            p.CloseScope();

            p.DecreasedIndent();
            return p.Result;
        }

        private void PrintEmptyEnumerator(ref Printer p, string elemTypeName)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public struct Enumerator : global::System.Collections.Generic.IEnumerator<{elemTypeName}>");
            p.OpenScope();
            {
                p.PrintLine($"public {elemTypeName} Current => throw new global::System.IndexOutOfRangeException();");
                p.PrintEndLine();
                p.PrintLine("public bool MoveNext() => false;");
                p.PrintEndLine();
                p.PrintLine("public void Reset() { }");
                p.PrintEndLine();
                p.PrintLine("public void Dispose() { }");
                p.PrintEndLine();
                p.PrintLine("object global::System.Collections.IEnumerator.Current => throw new global::System.IndexOutOfRangeException();");
            }
            p.CloseScope();
        }

        private void PrintEnumerator(ref Printer p, string elemTypeName)
        {
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public unsafe struct Enumerator : global::System.Collections.Generic.IEnumerator<{elemTypeName}>");
            p.OpenScope();
            {
                p.PrintLine("private readonly void* _ptr;");
                p.PrintLine("private int _index;");
                p.PrintLine($"private {elemTypeName}? _current;");
                p.PrintEndLine();
                p.PrintLine("public Enumerator(void* ptr)");
                p.OpenScope();
                {
                    p.PrintLine("_ptr = ptr;");
                    p.PrintLine("_index = 0;");
                    p.PrintLine("_current = default;");
                }
                p.CloseScope();
                p.PrintEndLine();
                p.PrintLine($"public {elemTypeName} Current");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine($"get => ({elemTypeName})_current!;");
                }
                p.CloseScope();
                p.PrintEndLine();
                p.PrintLine("public bool MoveNext()");
                p.OpenScope();
                {
                    p.PrintLine("if ((uint)_index < (uint)SIZE)");
                    p.OpenScope();
                    {
                        p.PrintLine($"var ptr = global::System.Runtime.CompilerServices.Unsafe.Add<{elemTypeName}>(_ptr, _index);");
                        p.PrintLine($"_current = global::System.Runtime.CompilerServices.Unsafe.AsRef<{elemTypeName}>(ptr);");
                        p.PrintLine("_index++;");
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                    p.PrintLine("_index = SIZE + 1;");
                    p.PrintLine("_current = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();
                p.PrintLine("public void Reset()");
                p.OpenScope();
                {
                    p.PrintLine("_index = 0;");
                    p.PrintLine("_current = default;");
                }
                p.CloseScope();
                p.PrintEndLine();
                p.PrintLine("public void Dispose() { }");
                p.PrintEndLine();
                p.PrintLine("object global::System.Collections.IEnumerator.Current");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get => _current!;");
                }
                p.CloseScope();
            }
            p.CloseScope();
        }
    }
}
