namespace AuthApi;

public static class MappingExtensions
{
    public static Guid ToGuid(this string source)
    {
        var isValid = Guid.TryParse(source, out var id);
        return !isValid ? Guid.Empty : id;
    }
}