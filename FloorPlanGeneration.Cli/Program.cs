using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FloorPlanGeneration;
using FloorPlanGeneration.Schema;

namespace FloorPlanGeneration.Cli
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            CliOptions options;
            if (!TryParseArgs(args, out options))
            {
                Console.Error.WriteLine("Usage: FloorPlanGeneration.Cli [--input path] [--output path]");
                return 64;
            }

            if (options.Help)
            {
                Console.Out.WriteLine("Usage: FloorPlanGeneration.Cli [--input path] [--output path]");
                return 0;
            }

            EngineOutput output;
            try
            {
                string json = ReadInput(options.InputPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    output = FailedOutput("cli.empty_input", "No JSON input was provided.");
                }
                else
                {
                    EngineInput input = JsonSerializer.Deserialize<EngineInput>(json, JsonOptions());
                    output = new FloorPlanEngine().Generate(input);
                }
            }
            catch (JsonException ex)
            {
                output = FailedOutput("cli.invalid_json", "Input JSON could not be parsed: " + ex.Message);
            }
            catch (Exception ex)
            {
                output = FailedOutput("cli.exception", "CLI execution failed: " + ex.Message);
            }

            WriteOutput(options.OutputPath, JsonSerializer.Serialize(output, JsonOptions()));
            return string.Equals(output.Status, "failed", StringComparison.OrdinalIgnoreCase) ? 2 : 0;
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        private static string ReadInput(string inputPath)
        {
            if (!string.IsNullOrWhiteSpace(inputPath))
            {
                return File.ReadAllText(inputPath);
            }

            return Console.In.ReadToEnd();
        }

        private static void WriteOutput(string outputPath, string json)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                Console.Out.WriteLine(json);
                return;
            }

            string directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, json + Environment.NewLine);
        }

        private static EngineOutput FailedOutput(string code, string message)
        {
            return new EngineOutput
            {
                ProjectId = "project",
                Status = "failed",
                Diagnostics = new List<Diagnostic>
                {
                    Diagnostic.Error(code, message)
                }
            };
        }

        private static bool TryParseArgs(string[] args, out CliOptions options)
        {
            options = new CliOptions();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "--help" || arg == "-h")
                {
                    options.Help = true;
                    continue;
                }

                if (arg == "--input")
                {
                    if (++i >= args.Length) return false;
                    options.InputPath = args[i];
                    continue;
                }

                if (arg == "--output")
                {
                    if (++i >= args.Length) return false;
                    options.OutputPath = args[i];
                    continue;
                }

                return false;
            }

            return true;
        }

        private sealed class CliOptions
        {
            public string InputPath { get; set; }
            public string OutputPath { get; set; }
            public bool Help { get; set; }
        }
    }
}
