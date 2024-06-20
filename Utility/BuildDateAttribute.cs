using System.Globalization;
using System.Reflection;

namespace BozoAIAggregator.Utility;

[AttributeUsage(AttributeTargets.Assembly)]
internal class BuildDateAttribute : Attribute
{
    public BuildDateAttribute(string value)
    {
        DateTime = DateTime.ParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    public DateTime DateTime { get; }

    public static DateTime GetBuildDate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string buildVersionMetadataPrefix = "+build";
        var attribute = assembly.GetCustomAttribute<BuildDateAttribute>();
        if (attribute?.DateTime == null) return default;
        return attribute.DateTime;
    }
}
