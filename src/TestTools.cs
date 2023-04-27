using System.Reflection;
using System.Reflection.Emit;

namespace InterfaceInstance;

public static class TestTools
{
    public static T CreateInterface<T>(object propertyValues)
    {
        _ = propertyValues ?? throw new ArgumentNullException(nameof(propertyValues));
        var interfaceType = typeof(T);

        if (!interfaceType.IsInterface)
        {
            throw new ArgumentException($"Type: {interfaceType.FullName} is not an Interface");
        }

        // Create an instance of interface
        TypeBuilder typeBuilder = CreateTypeBuilder(interfaceType.Name, interfaceType);
        Type dynamicType = typeBuilder.CreateType();
        T result = (T)Activator.CreateInstance(dynamicType);

        // Copy properties from the object to the interface object
        foreach (var property in propertyValues.GetType().GetProperties())
        {
            PropertyInfo interfaceProperty = result.GetType().GetProperty(property.Name);
            interfaceProperty.SetValue(result, property.GetValue(propertyValues));
        }

        return result;
    }

    private static TypeBuilder CreateTypeBuilder(string typeName, Type interfaceType)
    {
        AssemblyName assemblyName = new AssemblyName(typeName);
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(typeName);
        TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);
        MethodAttributes propertyAttibutes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;

        var properties = interfaceType
            .GetInterfaces().SelectMany(s => s.GetProperties())
            .Union(interfaceType.GetProperties());
        foreach (var property in properties)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + property.Name, property.PropertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_" + property.Name, propertyAttibutes, property.PropertyType, Type.EmptyTypes);
            ILGenerator getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);
            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + property.Name, propertyAttibutes, null, new Type[] { property.PropertyType });
            ILGenerator setIL = setMethodBuilder.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }

        typeBuilder.AddInterfaceImplementation(interfaceType);

        return typeBuilder;
    }
}
