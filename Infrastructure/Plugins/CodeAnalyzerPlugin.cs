using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Infrastructure.Plugins
{
    /// <summary>
    /// Analiza código C# usando Roslyn.
    /// Más confiable que pedirle al LLM que lo haga,
    /// porque Roslyn sí entiende la sintaxis real.
    /// </summary>
    public class CodeAnalyzerPlugin
    {
        private readonly ILogger<CodeAnalyzerPlugin> _logger;

        public CodeAnalyzerPlugin(ILogger<CodeAnalyzerPlugin> logger)
        {
            _logger = logger;
        }

        [KernelFunction("ExtractPublicMethods")]
        [Description("Extrae la lista de métodos públicos de un archivo C#. Úsalo para saber qué métodos necesitan tests.")]
        public Task<string> ExtractPublicMethodsAsync(
            [Description("Contenido completo del archivo C# a analizar")] string csharpCode)
        {
            _logger.LogInformation("[CodeAnalyzer] Extrayendo métodos públicos");

            try
            {
                var tree = CSharpSyntaxTree.ParseText(csharpCode);
                var root = tree.GetRoot();

                var methods = root
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)))
                    .Select(m => new
                    {
                        Name = m.Identifier.Text,
                        ReturnType = m.ReturnType.ToString(),
                        Parameters = m.ParameterList.ToString()
                    })
                    .ToList();

                if (!methods.Any())
                    return Task.FromResult("No se encontraron métodos públicos en este archivo.");

                var result = string.Join("\n", methods.Select(m =>
                    $"- {m.ReturnType} {m.Name}{m.Parameters}"));

                _logger.LogInformation("[CodeAnalyzer] Encontrados {Count} métodos públicos", methods.Count);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CodeAnalyzer] Error analizando código");
                return Task.FromResult($"ERROR: No se pudo analizar el código. {ex.Message}");
            }
        }

        [KernelFunction("DetectFramework")]
        [Description("Detecta si el proyecto usa xUnit, NUnit o MSTest como framework de tests.")]
        public Task<string> DetectFrameworkAsync(
            [Description("Contenido del archivo .csproj o de un archivo de tests existente")] string projectContent)
        {
            _logger.LogInformation("[CodeAnalyzer] Detectando framework de tests");

            if (projectContent.Contains("xunit", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult("xUnit");

            if (projectContent.Contains("nunit", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult("NUnit");

            if (projectContent.Contains("MSTest", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult("MSTest");

            // Default — xUnit es el más común en proyectos .NET modernos
            return Task.FromResult("xUnit");
        }

        [KernelFunction("ValidateSyntax")]
        [Description("Valida que el código de tests generado compila correctamente. OBLIGATORIO antes de hacer commit.")]
        public Task<string> ValidateSyntaxAsync(
            [Description("Código de tests generado que se quiere validar")] string testCode)
        {
            _logger.LogInformation("[CodeAnalyzer] Validando sintaxis del código generado");

            try
            {
                var tree = CSharpSyntaxTree.ParseText(testCode);
                var diagnostics = tree.GetDiagnostics()
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .ToList();

                if (!diagnostics.Any())
                {
                    _logger.LogInformation("[CodeAnalyzer] Sintaxis válida");
                    return Task.FromResult("VÁLIDO: El código no tiene errores de sintaxis.");
                }

                var errors = string.Join("\n", diagnostics.Select(d =>
                    $"Línea {d.Location.GetLineSpan().StartLinePosition.Line + 1}: {d.GetMessage()}"));

                _logger.LogWarning("[CodeAnalyzer] Errores de sintaxis encontrados:\n{Errors}", errors);

                return Task.FromResult($"INVÁLIDO: Se encontraron errores:\n{errors}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CodeAnalyzer] Error en validación");
                return Task.FromResult($"ERROR: No se pudo validar. {ex.Message}");
            }
        }
    }
}
