using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Willow.SourceGenerators;
using Willow.Speech;
using Willow.Speech.ScriptingInterface;

namespace Tests.Speech.ScriptingInterface;

public sealed class CommandSourceGeneratorTests
{
    [Fact]
    public void When_MethodDoesNotContainVoiceCommandAttribute_Ignored()
    {
        const string Method = """
                              public class Class
                              {
                                  public void EmptyMethod() {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var result = RunGenerators(compilation, out _);
        compilation.SyntaxTrees.Should().BeEquivalentTo(result.SyntaxTrees);
    }

    [Fact]
    public void When_MethodContainVoiceCommandAttributeButNotReturnsTask_Error()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public void EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        _ = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().Contain(VoiceCommandGenerator.IncorrectSignature);
    }

    [Fact]
    public void When_MethodContainVoiceCommandAttributeButTakesWrongParameterCount_Error()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _, string int) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        _ = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().Contain(VoiceCommandGenerator.IncorrectSignature);
    }

    [Fact]
    public void When_MethodContainVoiceCommandAttributeButTakesWrongParameterType_Error()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(int _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        _ = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().Contain(VoiceCommandGenerator.IncorrectSignature);
    }

    [Fact]
    public void When_VoiceCommandAttributePresentAndAllValid_VoiceCommandClassCreated()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethodVoiceCommand(VoiceCommandContext _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var result = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().BeEmpty();
        result.GetVoiceCommandClass().Should().NotBeNull();
    }

    [Fact]
    public void When_MethodContainsValidAttributes_CopiedOver()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  [Tag("hello"), Tag("world")]
                                  [Tag("something")]
                                  [Description("Desc")]
                                  [Name("name")]
                                  [Alias("hello low")]
                                  [Alias("hello high")]
                                  [ActivationMode("dictation")]
                                  [SupportedOss(nameof(SupportedOss.Windows))]
                                  public Task EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;

        var compilation = CreateCompilation(Method);

        var attributeLists = compilation.SyntaxTrees.SelectMany(static tree => tree.GetRoot().ChildNodes())
                                        .OfType<ClassDeclarationSyntax>()
                                        .Where(static syntax => syntax.Identifier.Text == "Class")
                                        .SelectMany(static syntax => syntax.Members)
                                        .OfType<MethodDeclarationSyntax>()
                                        .Where(static syntax => syntax.Identifier.Text == "EmptyMethod")
                                        .SelectMany(static syntax => syntax.AttributeLists)
                                        .SelectMany(static list => list.Attributes)
                                        .Select(static syntax => syntax.WithoutTrivia());

        var result = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().BeEmpty();
        result.GetVoiceCommandClass()
              .AttributeLists.SelectMany(static list => list.Attributes)
              .Select(static syntax => syntax.WithoutTrivia())
              .Where(static syntax => !syntax.Name.ToString().Contains("CompilerGenerated"))
              .All(syntax => attributeLists.Any(list => list.IsEquivalentTo(syntax)))
              .Should()
              .BeTrue();
    }

    [Fact]
    public void When_MethodContainsInvalidAttributes_Error()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  [MethodImpl(MethodImplOptions.InternalCall)]
                                  public Task EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        _ = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().Contain(VoiceCommandGenerator.InvalidAttributesDetected);
    }

    [Fact]
    public void When_EnclosingClassContainsMultipleConstructors_Error()
    {
        const string Method = """
                              public class Class
                              {
                                  public Class(string name, int number) {}
                                  public Class(string name) {}
                                  
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(int _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        _ = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().Contain(VoiceCommandGenerator.OnlySingleConstructorAllowed);
    }

    [Fact]
    public void When_EnclosingClassContainsFields_CopiedOver()
    {
        const string Method = """
                              public class Class
                              {
                                  private const string Hello = "hello";
                                  private const string Hello3 = "hello";
                                  public static int s_hello = 5;
                                  
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var fieldDeclarations = compilation.SyntaxTrees.SelectMany(static tree => tree.GetRoot().ChildNodes())
                                           .OfType<ClassDeclarationSyntax>()
                                           .Where(static syntax => syntax.Identifier.Text == "Class")
                                           .SelectMany(static syntax => syntax.Members)
                                           .OfType<FieldDeclarationSyntax>()
                                           .Select(static syntax => syntax.WithoutTrivia());

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .Members.OfType<FieldDeclarationSyntax>()
              .Select(static syntax => syntax.WithoutTrivia())
              .Should()
              .HaveCount(3)
              .And.Contain(syntax => fieldDeclarations.Any(syntax.IsEquivalentTo));
    }

    [Fact]
    public void When_EnclosingClassContainsConstructor_CopiedOver()
    {
        const string Method = """
                              public class Class
                              {
                                  public Class(string item) { item.ToString(); }
                                  
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var constructorDeclaration = compilation.SyntaxTrees.SelectMany(static tree => tree.GetRoot().ChildNodes())
                                                .OfType<ClassDeclarationSyntax>()
                                                .Where(static syntax => syntax.Identifier.Text == "Class")
                                                .SelectMany(static syntax => syntax.Members)
                                                .OfType<ConstructorDeclarationSyntax>()
                                                .First()
                                                .WithIdentifier(
                                                    SyntaxFactory.Identifier("EmptyMethodVoiceCommand").WithoutTrivia())
                                                .WithoutTrivia();

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .Members.OfType<ConstructorDeclarationSyntax>()
              .Select(static syntax => syntax.WithoutTrivia())
              .Should()
              .ContainSingle()
              .And.Contain(syntax => syntax.IsEquivalentTo(constructorDeclaration));
    }

    [Fact]
    public void When_EnclosingClassContainsProperties_CopiedOver()
    {
        const string Method = """
                              public class Class
                              {
                                  public string Hello => "hello";
                                  private string Hello2 => "hello";
                                  public VoiceCommandContext Hello { get; set; } = new();
                                  
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var propertyDeclarations = compilation.SyntaxTrees.SelectMany(static tree => tree.GetRoot().ChildNodes())
                                              .OfType<ClassDeclarationSyntax>()
                                              .Where(static syntax => syntax.Identifier.Text == "Class")
                                              .SelectMany(static syntax => syntax.Members)
                                              .OfType<PropertyDeclarationSyntax>()
                                              .Select(static syntax => syntax.WithoutTrivia());

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .Members.OfType<PropertyDeclarationSyntax>()
              .Select(static syntax => syntax.WithoutTrivia())
              .Where(static syntax => syntax.Identifier.Text != "InvocationPhrase")
              .Should()
              .HaveCount(3)
              .And.Contain(syntax => propertyDeclarations.Any(syntax.IsEquivalentTo));
    }

    [Fact]
    public void When_CreatingClass_StringFromVoiceCommandBecomesInvocationPhrase()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _) { return Task.CompletedTask; }
                              }
                              """;
        var compilation = CreateCompilation(Method);

        var syntaxLiteral = SyntaxFactory
                            .LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("hello"))
                            .WithoutTrivia();

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .Members.OfType<PropertyDeclarationSyntax>()
              .Where(static syntax => syntax.Identifier.Text == "InvocationPhrase")
              .Select(static syntax => syntax.ExpressionBody?.Expression.WithoutTrivia())
              .OfType<LiteralExpressionSyntax>()
              .Should()
              .ContainSingle()
              .And.Contain(syntax => syntax.IsEquivalentTo(syntaxLiteral));
    }

    [Fact]
    public void When_EnclosingClassContainsInvocationPhraseProperty_Overwritten()
    {
        const string Method = """
                              public class Class
                              {
                                  public string InvocationPhrase => "This Should Change";
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;

        var compilation = CreateCompilation(Method);

        var syntaxLiteral = SyntaxFactory
                            .LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("hello"))
                            .WithoutTrivia();

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .Members.OfType<PropertyDeclarationSyntax>()
              .Where(static syntax => syntax.Identifier.Text == "InvocationPhrase")
              .Select(static syntax => syntax.ExpressionBody?.Expression.WithoutTrivia())
              .OfType<LiteralExpressionSyntax>()
              .Should()
              .ContainSingle()
              .And.Contain(syntax => syntax.IsEquivalentTo(syntaxLiteral));
    }

    [Fact]
    public void When_EnclosingFileContainsUsing_UsingIsCopiedAlongsideOwnNamespace()
    {
        const string Method = """
                              using System;

                              namespace Hello.World;

                              public class Class
                              {
                                  public string InvocationPhrase => "This Should Change";
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;

        var compilation = CreateCompilation(Method);

        var usingDirective = compilation.SyntaxTrees.First()
                                        .GetRoot()
                                        .ChildNodes()
                                        .OfType<UsingDirectiveSyntax>()
                                        .First()
                                        .WithoutTrivia();

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .SyntaxTree.GetRoot()
              .ChildNodes()
              .OfType<UsingDirectiveSyntax>()
              .Select(static syntax => syntax.WithoutTrivia())
              .Should()
              .ContainSingle()
              .And.Contain(syntax => syntax.IsEquivalentTo(usingDirective));
    }

    [Fact]
    public void When_ClassInNamespace_ResultCommandAlsoInSameNamespace()
    {
        const string Method = """
                              using System;

                              namespace Hello.World;

                              public class Class
                              {
                                  public string InvocationPhrase => "This Should Change";
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;

        var compilation = CreateCompilation(Method);

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .SyntaxTree.GetRoot()
              .ChildNodes()
              .OfType<BaseNamespaceDeclarationSyntax>()
              .First()
              .Name.ToString()
              .Should()
              .BeEquivalentTo("Hello.World");
    }

    [Fact]
    public void When_MethodIsVoiceCommand_MethodContentsCopied()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _)
                                  {
                                      Console.WriteLine("Hello World!");
                                      return Task.CompletedTask;
                                  }
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var methodDeclaration = compilation.SyntaxTrees.SelectMany(static tree => tree.GetRoot().ChildNodes())
                                           .OfType<ClassDeclarationSyntax>()
                                           .Where(static syntax => syntax.Identifier.Text == "Class")
                                           .SelectMany(static syntax => syntax.Members)
                                           .OfType<MethodDeclarationSyntax>()
                                           .Where(static syntax => syntax.Identifier.Text == "EmptyMethod")
                                           .Select(static syntax => syntax.Body!)
                                           .First();

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .Members.OfType<MethodDeclarationSyntax>()
              .Select(static syntax => syntax.Body)
              .Should()
              .ContainSingle()
              .And.Contain(syntax => syntax!.IsEquivalentTo(methodDeclaration));
    }

    [Fact]
    public void When_MethodIsVoiceCommandExpression_MethodContentsCopied()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _) => Task.CompletedTask;
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var methodDeclaration = compilation.SyntaxTrees.SelectMany(static tree => tree.GetRoot().ChildNodes())
                                           .OfType<ClassDeclarationSyntax>()
                                           .Where(static syntax => syntax.Identifier.Text == "Class")
                                           .SelectMany(static syntax => syntax.Members)
                                           .OfType<MethodDeclarationSyntax>()
                                           .Where(static syntax => syntax.Identifier.Text == "EmptyMethod")
                                           .Select(static syntax => syntax.ExpressionBody!)
                                           .Select(static syntax => syntax.WithoutTrivia())
                                           .First();

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .Members.OfType<MethodDeclarationSyntax>()
              .Select(static syntax => syntax.ExpressionBody!)
              .Select(static syntax => syntax.WithoutTrivia())
              .Should()
              .ContainSingle()
              .And.Contain(syntax => syntax.IsEquivalentTo(methodDeclaration));
    }

    [Fact]
    public void When_MethodIsVoiceCommandAsync_MethodGeneratedIsAsync()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public async Task EmptyMethod(VoiceCommandContext _) {};
                              }
                              """;
        var compilation = CreateCompilation(Method);

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .Members.OfType<MethodDeclarationSyntax>()
              .Should()
              .ContainSingle()
              .And.Contain(static syntax =>
                               syntax.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.AsyncKeyword)));
    }

    [Fact]
    public void When_MethodNameDoesNotContainVoiceCommand_ClassNameIsXVoiceCommand()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethod(VoiceCommandContext _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var result = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().NotContain(VoiceCommandGenerator.IncorrectSignature);
        result.GetVoiceCommandClass().Should().NotBeNull();
    }

    [Fact]
    public void When_MethodNameContainsCommand_ClassNameIsXVoiceCommand()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethodCommand(VoiceCommandContext _) {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var result = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().NotContain(VoiceCommandGenerator.IncorrectSignature);
        result.GetVoiceCommandClass().Should().NotBeNull();
    }

    [Fact]
    public void When_NoMethodRequested_NoneCopied()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello")]
                                  public Task EmptyMethodCommand(VoiceCommandContext _) {}
                                  
                                  private void Test() {}
                              }
                              """;
        var compilation = CreateCompilation(Method);
        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .ChildNodes()
              .OfType<MethodDeclarationSyntax>()
              .Select(static syntax => syntax.Identifier.Text)
              .Should()
              .NotContain("Test");
    }

    [Fact]
    public void When_MethodRequested_IsCopied()
    {
        const string Method = """
                              public class Class
                              {
                                  [VoiceCommand("hello", RequiredMethods = new string[] {nameof(Test), nameof(Test2), nameof(Test3)})]
                                  public Task EmptyMethodCommand(VoiceCommandContext _) {}
                                  
                                  private void Test() {}
                                  public int Test2() { return 0; }
                                  public static void Test3(string int) { int++; }
                              }
                              """;
        var compilation = CreateCompilation(Method);

        var methodDeclarations = compilation.SyntaxTrees.SelectMany(static syntax => syntax.GetRoot().ChildNodes())
                                            .OfType<ClassDeclarationSyntax>()
                                            .SelectMany(static syntax => syntax.ChildNodes())
                                            .OfType<MethodDeclarationSyntax>()
                                            .Where(static syntax => syntax.Identifier.Text != "EmptyMethodCommand")
                                            .Select(static syntax => syntax.WithoutTrivia());

        var result = RunGenerators(compilation, out _);
        result.GetVoiceCommandClass()
              .ChildNodes()
              .OfType<MethodDeclarationSyntax>()
              .Select(static syntax => syntax.WithoutTrivia())
              .Where(static syntax => syntax.Identifier.Text != nameof(IVoiceCommand.ExecuteAsync))
              .All(syntax => methodDeclarations.Any(methods => methods.IsEquivalentTo(syntax)))
              .Should()
              .BeTrue();
    }

    [Fact]
    public void When_CreatingComplexCommand_NoErrorOrWarningLevelDiagnostics()
    {
        const string Method = """
                              public class Class
                              {
                                  private readonly int _field = 5;
                              
                                  public Class()
                                  { _field = 10; }
                              
                                  [VoiceCommand("hello", RequiredMethods = new string[] {nameof(Test), nameof(Test2), nameof(Test3)})]
                                  public Task EmptyMethodCommand(VoiceCommandContext context)
                                  {
                                      Test();
                                      var number = Test2();
                                      number = Test3(number);
                                      number = _field + 5;
                                      return Task.CompletedTask;
                                  }
                                  
                                  private void Test() {}
                                  public int Test2() { return 0; }
                                  public static int Test3(int input) { return input++ }
                              }
                              """;
        var compilation = CreateCompilation(Method);
        _ = RunGenerators(compilation, out var diagnostics);
        diagnostics.Should().BeEmpty();
    }

    private static CSharpGeneratorDriver CreateDriver()
    {
        return CSharpGeneratorDriver.Create(new VoiceCommandGenerator());
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        const string GlobalUsing = """
                                   global using global::Willow.Speech.ScriptingInterface.Attributes;
                                   global using global::Willow.Speech.ScriptingInterface.Models;
                                   global using global::System;
                                   global using global::System.Runtime;
                                   """;
        return CSharpCompilation.Create("compilation",
                                        [CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(GlobalUsing)],
                                        [
                                            MetadataReference.CreateFromFile(
                                                typeof(ISpeechAssemblyMarker).Assembly.Location),
                                            MetadataReference.CreateFromFile(
                                                typeof(IVoiceCommand).Assembly.Location),
                                            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                                            MetadataReference.CreateFromFile(
                                                Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!,
                                                             "System.Runtime.dll")),
                                        ],
                                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                                                                     true,
                                                                     warningLevel: 0));
    }

    private static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics)
    {
        CreateDriver().RunGeneratorsAndUpdateCompilation(compilation, out var compilationResult, out diagnostics);
        return compilationResult;
    }
}

file static class CompilationExtensions
{
    public static ClassDeclarationSyntax GetVoiceCommandClass(this Compilation compilation,
                                                              string className = "EmptyMethodVoiceCommand")
    {
        var finder = new VoiceCommandFinder(className);
        var found = compilation.SyntaxTrees.Select(tree =>
                               {
                                   finder.Visit(tree.GetRoot());
                                   return finder.Found;
                               })
                               .Where(static x => x is not null)
                               .FirstOrDefault();
        found.Should().NotBeNull();
        return found!;
    }
}

file sealed class VoiceCommandFinder : CSharpSyntaxWalker
{
    public ClassDeclarationSyntax? Found { get; private set; }
    private readonly string _methodName;

    public VoiceCommandFinder(string methodName)
    {
        _methodName = methodName;
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node.Identifier.Text == _methodName)
        {
            Found = node;
        }

        base.VisitClassDeclaration(node);
    }
}
