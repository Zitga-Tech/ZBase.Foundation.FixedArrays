using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading;
using ZBase.Foundation.SourceGen;

namespace ZBase.Foundation.FixedArrays
{
    [Generator]
    public class FixedArrayGenerator : IIncrementalGenerator
    {
        public const string INTERFACE_NAME = "IFixedArray";
        public const string ATTRIBUTE_NAME = "FixedArraySize";
        public const string FULL_SEMANTIC_ATTRIBUTE_NAME = "global::ZBase.Foundation.FixedArrays.FixedArraySizeAttribute";
        public const string FULL_SEMANTIC_INTERFACE_NAME = "global::ZBase.Foundation.FixedArrays.IFixedArray";
        public const string GENERATOR_NAME = nameof(FixedArrayGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsSyntaxMatch,
                transform: GetSemanticMatch
            ).Where(t => t is { });

            var compilationProvider = context.CompilationProvider;
            var combined = candidateProvider.Combine(compilationProvider).Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(
                    sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        public static bool IsSyntaxMatch(
              SyntaxNode syntaxNode
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (syntaxNode is not StructDeclarationSyntax structSyntax)
            {
                return false;
            }

            if (structSyntax.BaseList == null)
            {
                return false;
            }

            foreach (var baseType in structSyntax.BaseList.Types)
            {
                if (baseType.Type is GenericNameSyntax genericSyntax
                    && genericSyntax.Identifier.ValueText == INTERFACE_NAME
                    && genericSyntax.TypeArgumentList.Arguments.Count == 1
                )
                {
                    return true;
                }
            }

            return false;
        }

        public static FixedArrayDeclaration GetSemanticMatch(
              GeneratorSyntaxContext syntaxContext
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (syntaxContext.Node is not StructDeclarationSyntax structSyntax)
            {
                return null;
            }

            var semanticModel = syntaxContext.SemanticModel;

            TypeInfo? interfaceTypeInfo = null;
            TypeInfo? elementTypeInfo = null;

            foreach (var baseType in structSyntax.BaseList.Types)
            {
                if (baseType.Type is not GenericNameSyntax genericSyntax
                    || genericSyntax.TypeArgumentList.Arguments.Count != 1
                )
                {
                    continue;
                }

                var typeInfo = semanticModel.GetTypeInfo(baseType.Type, token);
                
                if (typeInfo.Type.ToFullName().StartsWith(FULL_SEMANTIC_INTERFACE_NAME))
                {
                    interfaceTypeInfo = typeInfo;
                    elementTypeInfo = semanticModel.GetTypeInfo(genericSyntax.TypeArgumentList.Arguments[0], token);
                    break;
                }
            }

            if (interfaceTypeInfo.HasValue == false || elementTypeInfo.HasValue == false)
            {
                return null;
            }

            var arraySize = 0;

            foreach (var list in structSyntax.AttributeLists)
            {
                foreach (var attr in list.Attributes)
                {
                    var typeInfo = semanticModel.GetTypeInfo(attr, token);

                    if (typeInfo.Type.ToFullName() != FULL_SEMANTIC_ATTRIBUTE_NAME)
                    {
                        continue;
                    }

                    if (attr.ArgumentList == null || attr.ArgumentList.Arguments.Count != 1)
                    {
                        continue;
                    }

                    if (attr.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literalSyntax
                        && literalSyntax.IsKind(SyntaxKind.NumericLiteralExpression)
                        && int.TryParse(literalSyntax.Token.ValueText, out var size)
                    )
                    {
                        arraySize = size;
                        goto RETURN;
                    }
                }
            }

            RETURN:
            return new FixedArrayDeclaration {
                Syntax = structSyntax,
                ArraySize = arraySize,
                InterfaceTypeInfo = interfaceTypeInfo.Value,
                ElementTypeInfo = elementTypeInfo.Value,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , FixedArrayDeclaration declaration
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (declaration == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var syntax = declaration.Syntax;
            var syntaxTree = syntax.SyntaxTree;

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var source = declaration.WriteCode();
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(compilation.Assembly.Name, GENERATOR_NAME);
                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , syntax
                    , source
                    , context.CancellationToken
                );

                context.AddSource(
                      syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax)
                    , outputSource
                );

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , syntax.GetLocation()
                        , sourceFilePath
                        , outputSource
                    );
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_FIXED_ARRAY_01"
                , "Fixed Array Generator Error"
                , "This error indicates a bug in the Fixed Array source generators. Error message: '{0}'."
                , "ZBase.Foundation.FixedArrays.FixedArrayAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}