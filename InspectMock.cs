using System;
using System.Reflection;

class Program {
    static void Main() {
        var asm = Assembly.LoadFrom("/Users/sivaprakashs/.nuget/packages/mockqueryable.moq/8.0.1/lib/net8.0/MockQueryable.Moq.dll");
        foreach (var type in asm.GetTypes()) {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                Console.WriteLine($"{type.Name}.{method.Name}");
            }
        }
    }
}
