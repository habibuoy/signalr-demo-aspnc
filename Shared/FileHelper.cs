using System.Text.Json;

namespace SimpleVote.Shared;

public static class FileHelper
{
    /// <summary>
    /// A json serialized options that prettifies the json and make
    /// keys' naming using camelCase format.
    /// </summary>
    public static JsonSerializerOptions defaultJsonSerializerOptions = new()
    {
        // prettify
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Load a json file and deserialize its content into object of type <typeparamref name="T"/>.
    /// If the file does not exist, it will be created, and the object will be created from
    /// the object factory.
    /// </summary>
    /// <typeparam name="T">A class</typeparam>
    /// <param name="path">Path to json file, cannot be null or empty</param>
    /// <param name="objectFactory">Function to create the object</param>
    /// <param name="jsonSerializerOptions">If you don't know what to pass, you can leave it empty 
    /// or pass the <see cref="defaultJsonSerializerOptions"/></param>
    /// <returns>A tuple of the object created if the process is successful,
    /// null object otherwise, and a message of why the process was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static (T? result, string? message) LoadOrCreateFromJsonFile<T>(string path, Func<T> objectFactory,
        JsonSerializerOptions? jsonSerializerOptions = null) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(objectFactory);

        string? message = null;
        T? result = null;

        try
        {
            using var file = File.Open(path, FileMode.OpenOrCreate);
            result = JsonSerializer.Deserialize<T>(file, jsonSerializerOptions);
        }
        catch (JsonException)
        {
            result = objectFactory();
            message = "File was not found and created by the factory";
            if (result != null)
            {
                SaveToJsonFile(path, result, jsonSerializerOptions);
            }
        }
        catch (DirectoryNotFoundException)
        {
            message = $"Directory {Path.GetDirectoryName(path)} not found";
        }
        catch (IOException ioEx)
        {
            message = $"IO process error: {ioEx.Message}";
        }
        catch (UnauthorizedAccessException)
        {
            message = $"Not permitted to access file {path}";
        }
        catch (Exception ex)
        {
            message = $"Unexpected error: {ex}";
        }

        return (result, message);
    }


    /// <summary>
    /// Save the object <typeparamref name="T"/> to a json file.
    /// </summary>
    /// <typeparam name="T">A class</typeparam>
    /// <param name="path">Path to json file, cannot be null or empty</param>
    /// <param name="obj">Object to be saved, cannot be null</param>
    /// <param name="jsonSerializerOptions">If you don't know what to pass, you can leave it empty 
    /// or pass the <see cref="defaultJsonSerializerOptions"/></param>
    /// <returns>A tuple of a boolean indicating whether the process is successful or not 
    /// and a message of why the process was not successful</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static (bool isSuccess, string? message) SaveToJsonFile<T>(string path, T obj,
        JsonSerializerOptions? jsonSerializerOptions = null) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(obj);

        bool success = false;
        string? message = null;

        try
        {
            var jsonSerialized = JsonSerializer.Serialize(obj, jsonSerializerOptions);
            File.WriteAllText(path, jsonSerialized);
            success = true;
        }
        catch (NotSupportedException nsEx)
        {
            message = $"Operation not supported: {nsEx}";
        }
        catch (DirectoryNotFoundException)
        {
            message = $"Directory {Path.GetDirectoryName(path)} not found";
        }
        catch (IOException ioEx)
        {
            message = $"IO process error: {ioEx.Message}";
        }
        catch (UnauthorizedAccessException)
        {
            message = $"Not permitted to access file {path}";
        }
        catch (Exception ex)
        {
            message = $"Unexpected error: {ex}";
        }

        return (success, message);
    }
}