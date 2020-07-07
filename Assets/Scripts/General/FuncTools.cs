using System.Collections.Generic;
using System;

public static class FuncTools
{
    static readonly Dictionary<string, Func<bool>> funcTypes = new Dictionary<string, Func<bool>>();

    // Create class from text name (e.g. json file)
    public static T CreateClassByName<T>(string className)
    {
        var obj = Type.GetType(className);
        if (obj == null)
            return default;

        return (T)GetNewObject(Type.GetType(className));
    }
    
    // Simple functions returning bool, callable with text keys
    public static void AddFunc(string name, Func<bool> func)
    {
        funcTypes[name] = func;
    }

    public static Func<bool> GetFunc(string name)
    {
        if (funcTypes.ContainsKey(name))
            return funcTypes[name];
        else
            return delegate () { return true; };
    }

    public static bool ExecFunc(string name)
    {
        return GetFunc(name)();
    }

    private static object GetNewObject(Type t)
    {
        try
        {
            return t.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
        }
        catch
        {
            return null;
        }
    }

}
