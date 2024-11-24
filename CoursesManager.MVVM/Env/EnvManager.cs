using System.IO;
using dotenv.net;
using dotenv.net.Utilities;

namespace CoursesManager.MVVM.Env;

public class EnvManager<T>
    where T : new()
{
    private static readonly Lazy<T> _values = new(() =>
    {
        var model = new T();

        DotEnv.Fluent()
            .WithExceptions()
            .WithEnvFiles(FindEnvFiles().ToArray())
            .Load();

        LoadValues(model);
        return model;
    });

    public static T Values => _values.Value;

    private EnvManager() { }

    private static void LoadValues(T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var envFields = model.GetType().GetFields().Where(f => f.IsPublic);

        foreach (var field in envFields)
        {
            var envKey = field.Name;

            if (!EnvReader.HasValue(envKey)) throw new Exception($"Environment value with key: {envKey} not found.");

            object value = field.FieldType.Name switch
            {
                nameof(Int32) => EnvReader.GetIntValue(envKey),
                nameof(Boolean) => EnvReader.GetBooleanValue(envKey),
                nameof(Decimal) => EnvReader.GetDecimalValue(envKey),
                nameof(Double) => EnvReader.GetDoubleValue(envKey),
                nameof(String) => EnvReader.GetStringValue(envKey),
                _ => throw new Exception($"Invalid type in model: {field.FieldType}")
            };

            field.SetValue(model, value);
        }
    }

    private static List<string> FindEnvFiles()
    {
        var envFiles = new List<string>();
        var startDirectory = Directory.GetCurrentDirectory();

        try
        {
            var files = Directory.GetFiles(startDirectory, "*.env", SearchOption.AllDirectories);

            envFiles.AddRange(files);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while searching for .env files: {ex.Message}");
        }

        return envFiles;
    }
}