#pragma warning disable CS1998

using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using MagicOnion.Generator.CodeAnalysis;
using MagicOnion.Generator.CodeGen;
using MagicOnion.Generator.Utils;
using MagicOnion.Generator.CodeGen.Extensions;
using MagicOnion.Generator.Internal;

namespace MagicOnion.Generator;

public class MagicOnionCompiler
{
    static readonly Encoding NoBomUtf8 = new UTF8Encoding(false);

    readonly IMagicOnionGeneratorLogger logger;
    readonly CancellationToken cancellationToken;

    public MagicOnionCompiler(IMagicOnionGeneratorLogger logger, CancellationToken cancellationToken)
    {
        this.logger = logger;
        this.cancellationToken = cancellationToken;
    }

    public async Task GenerateFileAsync(
        string input,
        string output,
        bool omitUnityAttribute,
        string @namespace,
        string conditionalSymbol,
        string userDefinedMessagePackFormattersNamespace)
    {
        // Prepare args
        var namespaceDot = string.IsNullOrWhiteSpace(@namespace) ? string.Empty : @namespace + ".";
        var conditionalSymbols = conditionalSymbol?.Split(',') ?? Array.Empty<string>();

        // Generator Start...
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:Input: {input}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:Output: {output}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:OmitUnityAttribute: {omitUnityAttribute}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:Namespace: {@namespace}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:ConditionalSymbol: {conditionalSymbol}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Option:UserDefinedMessagePackFormattersNamespace: {userDefinedMessagePackFormattersNamespace}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] Assembly version: {typeof(MagicOnionCompiler).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.OSDescription: {RuntimeInformation.OSDescription}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.ProcessArchitecture: {RuntimeInformation.ProcessArchitecture}");
        logger.Trace($"[{nameof(MagicOnionCompiler)}] RuntimeInformation.FrameworkDescription: {RuntimeInformation.FrameworkDescription}");

        var sw = Stopwatch.StartNew();
        logger.Information("Project Compilation Start:" + input);
        var compilation = await PseudoCompilation.CreateFromProjectAsync(new[] { input }, conditionalSymbols, logger, cancellationToken);
        logger.Information("Project Compilation Complete:" + sw.Elapsed.ToString());

        sw.Restart();
        logger.Information("Collect services and methods Start");
        var collector = new MethodCollector(logger);
        var serviceCollection = collector.Collect(compilation);
        logger.Information("Collect services and methods Complete:" + sw.Elapsed.ToString());
            
        sw.Restart();
        logger.Information("Collect serialization information Start");
        var serializationInfoCollector = new SerializationInfoCollector(logger);
        var serializationInfoCollection = serializationInfoCollector.Collect(serviceCollection, userDefinedMessagePackFormattersNamespace);
        logger.Information("Collect serialization information Complete:" + sw.Elapsed.ToString());

        logger.Information("Output Generation Start");
        sw.Restart();

        var resolverTemplate = new ResolverTemplate()
        {
            Namespace = namespaceDot + "Resolvers",
            FormatterNamespace = namespaceDot + "Formatters",
            ResolverName = "MagicOnionResolver",
            RegisterInfos = serializationInfoCollection.RequireRegistrationFormatters,
        };

        var registerTemplate = new RegisterTemplate
        {
            Namespace = @namespace,
            Services = serviceCollection.Services,
            Hubs = serviceCollection.Hubs,
            OmitUnityAttribute = omitUnityAttribute,
        };

        if (Path.GetExtension(output) == ".cs")
        {
            var enumTemplates = serializationInfoCollection.Enums
                .GroupBy(x => x.Namespace)
                .OrderBy(x => x.Key)
                .Select(x => new EnumTemplate()
                {
                    Namespace = namespaceDot + "Formatters",
                    EnumSerializationInfos = x.ToArray()
                })
                .ToArray();

            var clientTexts = StaticMagicOnionClientGenerator.Build(serviceCollection.Services);

            var hubTexts = serviceCollection.Hubs
                .GroupBy(x => x.ServiceType.Namespace)
                .OrderBy(x => x.Key)
                .Select(x => new HubTemplate()
                {
                    Namespace = x.Key,
                    Hubs = x.ToArray(),
                })
                .ToArray();

            var sb = new StringBuilder();
            sb.AppendLine("/// <auto-generated />");
            sb.AppendLine(registerTemplate.TransformText());
            sb.AppendLine(resolverTemplate.TransformText());
            foreach (var item in enumTemplates)
            {
                sb.AppendLine(item.TransformText());
            }

            sb.AppendLine(clientTexts);

            foreach (var item in hubTexts)
            {
                sb.AppendLine(item.TransformText());
            }

            Output(output, sb.ToString());
        }
        else
        {
            Output(NormalizePath(output, registerTemplate.Namespace, "MagicOnionInitializer"), WithAutoGenerated(registerTemplate.TransformText()));
            Output(NormalizePath(output, resolverTemplate.Namespace, resolverTemplate.ResolverName), WithAutoGenerated(resolverTemplate.TransformText()));

            foreach (var enumSerializationInfo in serializationInfoCollection.Enums)
            {
                var x = new EnumTemplate()
                {
                    Namespace = namespaceDot + "Formatters",
                    EnumSerializationInfos = new[] { enumSerializationInfo }
                };

                Output(NormalizePath(output, x.Namespace, enumSerializationInfo.Name + "Formatter"), WithAutoGenerated(x.TransformText()));
            }

            foreach (var service in serviceCollection.Services)
            {
                var x = StaticMagicOnionClientGenerator.Build(new[] { service });
                Output(NormalizePath(output, service.ServiceType.Namespace, service.GetClientName()), WithAutoGenerated(x));
            }

            foreach (var hub in serviceCollection.Hubs)
            {
                var x = new HubTemplate()
                {
                    Namespace = hub.ServiceType.Namespace,
                    Hubs = new[] { hub }
                };

                Output(NormalizePath(output, hub.ServiceType.Namespace, hub.GetClientName()), WithAutoGenerated(x.TransformText()));
            }
        }

        if (serviceCollection.Services.Count == 0 && serviceCollection.Hubs.Count == 0)
        {
            logger.Information("Generated result is empty, unexpected result?");
        }

        logger.Information("Output Generation Complete:" + sw.Elapsed.ToString());
    }

    static string NormalizePath(string dir, string ns, string className)
    {
        return Path.Combine(dir, $"{ns}_{className}".Replace(".", "_").Replace("global::", string.Empty) + ".cs");
    }

    static string WithAutoGenerated(string s)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/// <auto-generated />");
        sb.AppendLine(s);
        return sb.ToString();
    }

    static string NormalizeNewLines(string content)
    {
        // The T4 generated code may be text with mixed line ending types. (CR + CRLF)
        // We need to normalize the line ending type in each Operating Systems. (e.g. Windows=CRLF, Linux/macOS=LF)
        return content.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
    }

    void Output(string path, string text)
    {
        path = path.Replace("global::", "");

        logger.Information($"Write to {path}");

        var fi = new FileInfo(path);
        if (!fi.Directory.Exists)
        {
            fi.Directory.Create();
        }

        System.IO.File.WriteAllText(path, NormalizeNewLines(text), NoBomUtf8);
    }
}
