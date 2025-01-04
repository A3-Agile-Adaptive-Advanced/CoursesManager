using System.IO;
using dotenv.net;
using dotenv.net.Utilities;

namespace CoursesManager.MVVM.Env;

public class EnvManager<T>
    where T : new()
{
    public static string EnvFolderPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CoursesManager");

    private static Lazy<T> _values = new(ValueFactory);

    private static T ValueFactory()
    {
        var model = new T();

        var envfiles = FindEnvFiles();

        if (envfiles.Count == 0)
        {
            return model;
        }

        DotEnv.Fluent()
            .WithExceptions()
            .WithEnvFiles(envfiles.ToArray())
            .Load();

        LoadValues(model);
        return model;
    }

    public static T Values => _values.Value;

    private static void LoadValues(T model)
    {
        var envFields = model!.GetType().GetFields().Where(f => f.IsPublic);

        foreach (var field in envFields)
        {
            var envKey = field.Name;

            if (!EnvReader.HasValue(envKey))
            {
                Console.WriteLine($"Waarde voor sleutel '{envKey}' niet gevonden in .env.");
                throw new Exception($"Environment value met sleutel: {envKey} niet gevonden.");
            }

            object value = field.FieldType.Name switch
            {
                nameof(Int32) => EnvReader.GetIntValue(envKey),
                nameof(Boolean) => EnvReader.GetBooleanValue(envKey),
                nameof(String) => EnvReader.GetStringValue(envKey),
                _ => throw new Exception($"Invalid type in model: {field.FieldType}")
            };

            field.SetValue(model, value);
        }
    }

    public static void Save()
    {
        Dictionary<string, string> envData = new();

        var envFields = Values!.GetType().GetFields().Where(f => f.IsPublic);

        foreach (var field in envFields)
        {
            var envKey = field.Name;
            var value = field.GetValue(Values)?.ToString();

            envData[envKey] = value ?? string.Empty;
        }

        SaveToFile(envData);
    }

    public static void Reset()
    {
        _values = new Lazy<T>(ValueFactory);
    }

    public static void SaveToFile(Dictionary<string, string> envdata)
    {
        var envFilePath = Path.Combine(EnvFolderPath, ".env");

        try
        {
            using var writer = new StreamWriter(envFilePath);
            foreach (var kvp in envdata)
            {
                writer.WriteLine($"{kvp.Key}={kvp.Value}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while saving .env file: {ex.Message}");
        }
    }

    private static List<string> FindEnvFiles()
    {
        var envFiles = new List<string>();

        try
        {
            var files = Directory.GetFiles(EnvFolderPath, "*.env", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                Console.WriteLine("Geen .env-bestanden gevonden in de huidige directory.");
            }
            else
            {
                Console.WriteLine($"Gevonden .env-bestand(en): {string.Join(", ", files)}");
                envFiles.AddRange(files);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij het zoeken naar .env-bestanden: {ex.Message}");
        }

        return envFiles;
    }
}