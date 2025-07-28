using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class SessionExtensions
{
    /// <summary>
    /// Stores an object in session by serializing it to JSON.
    /// </summary>
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        var json = JsonConvert.SerializeObject(value);
        session.SetString(key, json);
    }

    /// <summary>
    /// Retrieves an object from session by deserializing it from JSON.
    /// </summary>
    public static T? GetObject<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        return json == null ? default : JsonConvert.DeserializeObject<T>(json);
    }
}
