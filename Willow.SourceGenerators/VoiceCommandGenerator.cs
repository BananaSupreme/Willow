using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Willow.SourceGenerators;

[Generator]
public class VoiceCommandGenerator : IIncrementalGenerator
{
    private const string VoiceCommandGeneratorDiagnosticId = "VCMD";

    public static Diagnostic IncorrectSignature =>
        Diagnostic.Create(VoiceCommandGeneratorDiagnosticId,
                          $"{VoiceCommandGeneratorDiagnosticId}001",
                          "Method must take in as a parameter VoiceCommandContext and return Task like the ExecuteAsync function of IVoiceCommand.",
                          DiagnosticSeverity.Error,
                          DiagnosticSeverity.Error,
                          true,
                          0);

    public static Diagnostic OnlySingleConstructorAllowed =>
        Diagnostic.Create(VoiceCommandGeneratorDiagnosticId,
                          $"{VoiceCommandGeneratorDiagnosticId}002",
                          "Method must be in a class containing a maximum of 1 constructor.",
                          DiagnosticSeverity.Error,
                          DiagnosticSeverity.Error,
                          true,
                          0);

    public static Diagnostic MissingRequestedMethods =>
        Diagnostic.Create(VoiceCommandGeneratorDiagnosticId,
                          $"{VoiceCommandGeneratorDiagnosticId}003",
                          "Could not find requested method in containing class, ensure all methods requested are in the class enclosing this method.",
                          DiagnosticSeverity.Error,
                          DiagnosticSeverity.Error,
                          true,
                          0);

    public static Diagnostic InvalidAttributesDetected =>
        Diagnostic.Create(VoiceCommandGeneratorDiagnosticId,
                          $"{VoiceCommandGeneratorDiagnosticId}004",
                          "Only attributes deriving the IVoiceCommandDescriptor interface are allowed, attributes are copied into a new class and would not affect the execution of the method for a more fine grained control consider using a IVoiceCommand class.",
                          DiagnosticSeverity.Error,
                          DiagnosticSeverity.Error,
                          true,
                          0);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var voiceCommands = context.SyntaxProvider
                                   .CreateSyntaxProvider(static (node, _) => node is MethodDeclarationSyntax,
                                                         GetMethodSymbol)
                                   .Where(ContainsVoiceCommandAttribute);
        context.RegisterSourceOutput(voiceCommands, GenerateOutput);
    }

    private static void GenerateOutput(SourceProductionContext sourceProductionContext,
                                       (IMethodSymbol MethodSymbol, TypeDeclarationSyntax EnclosingClass) input)
    {
        var voiceCommandMethodInfo = GetVoiceCommandMethodInfo(input, sourceProductionContext);
        sourceProductionContext.AddSource($"{voiceCommandMethodInfo.MethodName}.g.cs",
                                          $$"""
                                            #nullable enable

                                            {{string.Join<UsingDirectiveSyntax>("\r\n", voiceCommandMethodInfo.UsingDirectiveSyntax)}}
                                            namespace {{(voiceCommandMethodInfo.Namespace ?? $"Willow.Generated.{GetRandomChars()}")}};

                                            [global::System.Runtime.CompilerServices.CompilerGenerated]
                                            {{string.Join<AttributeListSyntax>("\r\n\t", voiceCommandMethodInfo.AttributeLists)}}
                                            public class {{voiceCommandMethodInfo.MethodName}} : global::Willow.Speech.ScriptingInterface.Abstractions.IVoiceCommand
                                            {
                                                {{string.Join<PropertyDeclarationSyntax>("\r\n\t", voiceCommandMethodInfo.PropertyDeclarationSyntax)}}
                                            
                                                {{string.Join<FieldDeclarationSyntax>("\r\n\t", voiceCommandMethodInfo.FieldDeclarationSyntax)}}
                                            
                                                {{(voiceCommandMethodInfo.ConstructorDeclarationSyntax is not null
                                                       ? voiceCommandMethodInfo.ConstructorDeclarationSyntax
                                                       : string.Empty)}}
                                                       
                                                public string InvocationPhrase => "{{voiceCommandMethodInfo.InvocationPhrase}}";
                                                
                                                public{{(input.MethodSymbol.IsAsync ? " async " : " ")}}global::System.Threading.Tasks.Task ExecuteAsync(global::Willow.Speech.ScriptingInterface.Models.VoiceCommandContext context)
                                                {{voiceCommandMethodInfo.MethodBody}}
                                                
                                                {{string.Join<MethodDeclarationSyntax>("\r\n\t\r\n\t", voiceCommandMethodInfo.RequestedMethods)}}
                                            }
                                            """);
    }

    private static VoiceCommandMethodInfo GetVoiceCommandMethodInfo(
        (IMethodSymbol MethodSymbol, TypeDeclarationSyntax EnclosingClass) input,
        SourceProductionContext sourceProductionContext)
    {
        var voiceCommandAttribute = GetVoiceCommandAttribute(input.MethodSymbol)!;
        var methodName = input.MethodSymbol.Name.AsSpan().GetTypeNameWithoutVoiceCommandEndings().ToString();
        methodName = $"{methodName}VoiceCommand";
        var invocationPhrase = (string)voiceCommandAttribute.ConstructorArguments[0].Value!;
        return new VoiceCommandMethodInfo(invocationPhrase,
                                          methodName,
                                          GetNamespace(input.EnclosingClass),
                                          GetConstructor(input.EnclosingClass, methodName, sourceProductionContext),
                                          input.EnclosingClass.Members.OfType<FieldDeclarationSyntax>().ToArray(),
                                          GetProperties(input.EnclosingClass),
                                          GetUsingDirectives(input.EnclosingClass),
                                          GetAttributes(input.MethodSymbol, sourceProductionContext),
                                          GetMethodBody(input.MethodSymbol, sourceProductionContext),
                                          GetRequestedMethods(voiceCommandAttribute,
                                                              input.EnclosingClass,
                                                              sourceProductionContext));
    }

    private static MethodDeclarationSyntax[] GetRequestedMethods(AttributeData voiceCommandAttribute,
                                                                 TypeDeclarationSyntax enclosingClass,
                                                                 SourceProductionContext sourceProductionContext)
    {
        if (voiceCommandAttribute.NamedArguments.Length == 0
            && !voiceCommandAttribute.NamedArguments.Any(CalledRequiredMethods))
        {
            return [];
        }


        var methods = voiceCommandAttribute.NamedArguments.First(CalledRequiredMethods)
                                           .Value.Values.Select(static s => (string)s.Value!)
                                           .Select(functionName =>
                                                       enclosingClass.ChildNodes()
                                                                     .OfType<MethodDeclarationSyntax>()
                                                                     .FirstOrDefault(syntax => syntax.Identifier.Text
                                                                         == functionName))
                                           .ToArray();

        if (methods.Any(static m => m is null))
        {
            sourceProductionContext.ReportDiagnostic(MissingRequestedMethods);
            return [];
        }

        return methods;

        static bool CalledRequiredMethods(KeyValuePair<string, TypedConstant> key)
        {
            return key.Key == "RequiredMethods";
        }
    }

    private static CSharpSyntaxNode GetMethodBody(IMethodSymbol methodSymbol,
                                                  SourceProductionContext sourceProductionContext)
    {
        if (methodSymbol.ReturnType.Name != "Task"
            || methodSymbol.Parameters.Length != 1
            || methodSymbol.Parameters[0].Type.Name != "VoiceCommandContext")
        {
            sourceProductionContext.ReportDiagnostic(IncorrectSignature);
        }

        var methodSyntax = GetMethodSyntax(methodSymbol);
        return methodSyntax.Body as CSharpSyntaxNode
               ?? methodSyntax.ExpressionBody as CSharpSyntaxNode
               ?? throw new InvalidOperationException("Should not reach here");
    }

    private static AttributeListSyntax[] GetAttributes(IMethodSymbol methodSymbol,
                                                       SourceProductionContext sourceProductionContext)
    {
        if (methodSymbol.GetAttributes().Any(static x => !ContainsVoiceCommandDescriptorInterface(x)))
        {
            sourceProductionContext.ReportDiagnostic(InvalidAttributesDetected);
        }

        var methodSyntax = GetMethodSyntax(methodSymbol);
        return methodSyntax.AttributeLists.ToArray();
    }

    private static bool ContainsVoiceCommandDescriptorInterface(AttributeData a)
    {
        return a.AttributeClass?.Interfaces.Select(static i => i.Name).Contains("IVoiceCommandDescriptor") ?? false;
    }

    private static UsingDirectiveSyntax[] GetUsingDirectives(TypeDeclarationSyntax enclosingClass)
    {
        return enclosingClass.SyntaxTree.GetRoot().ChildNodes().OfType<UsingDirectiveSyntax>().ToArray();
    }

    private static string? GetNamespace(TypeDeclarationSyntax enclosingClass)
    {
        return enclosingClass.SyntaxTree.GetRoot()
                             .ChildNodes()
                             .OfType<BaseNamespaceDeclarationSyntax>()
                             .FirstOrDefault()
                             ?.Name.ToString();
    }

    private static ConstructorDeclarationSyntax? GetConstructor(TypeDeclarationSyntax enclosingClass,
                                                                string methodName,
                                                                SourceProductionContext sourceProductionContext)
    {
        var constructors = enclosingClass.Members.OfType<ConstructorDeclarationSyntax>().ToArray();
        if (constructors.Length > 1)
        {
            sourceProductionContext.ReportDiagnostic(OnlySingleConstructorAllowed);
        }

        return constructors.FirstOrDefault()?.WithIdentifier(Identifier(methodName));
    }

    private static PropertyDeclarationSyntax[] GetProperties(TypeDeclarationSyntax enclosingClass)
    {
        return enclosingClass.Members.OfType<PropertyDeclarationSyntax>()
                             .Where(static syntax => syntax.Identifier.Text != "InvocationPhrase")
                             .ToArray();
    }

    private static MethodDeclarationSyntax GetMethodSyntax(IMethodSymbol methodSymbol)
    {
        var methodSyntax = methodSymbol.DeclaringSyntaxReferences.Select(static syntax => syntax.GetSyntax())
                                       .OfType<MethodDeclarationSyntax>()
                                       .First();
        return methodSyntax;
    }

    private static (IMethodSymbol MethodSymbol, TypeDeclarationSyntax ClassDeclerationSyntax) GetMethodSymbol(
        GeneratorSyntaxContext node,
        CancellationToken cancellationToken)
    {
        var methodSymbol = (IMethodSymbol)node.SemanticModel.GetDeclaredSymbol(node.Node, cancellationToken)!;
        return (methodSymbol,
                (TypeDeclarationSyntax)methodSymbol.ContainingType!.DeclaringSyntaxReferences[0].GetSyntax());
    }

    private static bool ContainsVoiceCommandAttribute(
        (IMethodSymbol MethodSymbol, TypeDeclarationSyntax ClassDeclerationSyntax) input)
    {
        return GetVoiceCommandAttribute(input.MethodSymbol) is not null;
    }

    private static AttributeData? GetVoiceCommandAttribute(IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributes()
                           .FirstOrDefault(static x => x.AttributeClass?.Name == "VoiceCommandAttribute");
    }

    private static string GetRandomChars()
    {
        var random = new Random();
        var chars = Enumerable.Repeat(0, 20)
                              .Select(_ => random.Next(65, 91) + (random.Next(0, 2) * 32))
                              .Select(static x => (char)x)
                              .ToArray();

        return new string(chars);
    }

    private readonly record struct VoiceCommandMethodInfo
    {
        public VoiceCommandMethodInfo(string invocationPhrase,
                                      string methodName,
                                      string? ns,
                                      ConstructorDeclarationSyntax? constructorDeclarationSyntax,
                                      FieldDeclarationSyntax[] fieldDeclarationSyntax,
                                      PropertyDeclarationSyntax[] propertyDeclarationSyntax,
                                      UsingDirectiveSyntax[] usingDirectiveSyntax,
                                      AttributeListSyntax[] attributeLists,
                                      CSharpSyntaxNode methodBody,
                                      MethodDeclarationSyntax[] requestedMethods)
        {
            InvocationPhrase = invocationPhrase;
            MethodName = methodName;
            Namespace = ns;
            ConstructorDeclarationSyntax = constructorDeclarationSyntax;
            FieldDeclarationSyntax = fieldDeclarationSyntax;
            PropertyDeclarationSyntax = propertyDeclarationSyntax;
            UsingDirectiveSyntax = usingDirectiveSyntax;
            AttributeLists = attributeLists;
            MethodBody = methodBody;
            RequestedMethods = requestedMethods;
        }

        public string InvocationPhrase { get; }
        public string MethodName { get; }
        public string? Namespace { get; }
        public ConstructorDeclarationSyntax? ConstructorDeclarationSyntax { get; }
        public FieldDeclarationSyntax[] FieldDeclarationSyntax { get; }
        public PropertyDeclarationSyntax[] PropertyDeclarationSyntax { get; }
        public UsingDirectiveSyntax[] UsingDirectiveSyntax { get; }
        public AttributeListSyntax[] AttributeLists { get; }
        public CSharpSyntaxNode MethodBody { get; }
        public MethodDeclarationSyntax[] RequestedMethods { get; }
    }
}
