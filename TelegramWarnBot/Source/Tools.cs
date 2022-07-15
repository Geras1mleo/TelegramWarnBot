namespace TelegramWarnBot;

public static class Tools
{
    public static readonly Regex CardNumberRegex = new(@"\d{4} ?\d{4} ?\d{4} ?\d{4}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    private static readonly Dictionary<Type, MethodInfo[]> methodsDict = new();
    public static MethodInfo ResolveMethod(Type type, string prefix)
    {
        if (!methodsDict.TryGetValue(type, out var cachedMethods))
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            methodsDict.Add(type, methods);
            return methods.FirstOrDefault(m => m.Name.ToLower().Equals(prefix));
        }

        return cachedMethods.FirstOrDefault(m => m.Name.ToLower().Equals(prefix));
    }
}