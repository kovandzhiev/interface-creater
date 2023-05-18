using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

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
        var typeBuilder = CreateTypeBuilder(interfaceType.Name, interfaceType);
        var dynamicType = typeBuilder.CreateType();
        var result = (T)Activator.CreateInstance(dynamicType);

        // Copy properties from the object to the interface object
        foreach (var property in propertyValues.GetType().GetProperties())
        {
            if (CheckIfAnonymousType(property.PropertyType))
            {
                // Prepare and make a recursion call
                var genericMethodInfo = typeof(TestTools).GetMethod("CreateInterface", BindingFlags.Static | BindingFlags.Public);

                var interfacePropertyType = interfaceType.GetProperty(property.Name).PropertyType;
                var constructedMethodInfo = genericMethodInfo.MakeGenericMethod(interfacePropertyType);

                object[] arguments = { property.GetValue(propertyValues) };
                var anonymousPropertyInstance = constructedMethodInfo.Invoke(null, arguments);

                var interfaceProperty = result.GetType().GetProperty(property.Name);
                interfaceProperty.SetValue(result, anonymousPropertyInstance);
            }
            else
            {
                var interfaceProperty = result.GetType().GetProperty(property.Name);

                interfaceProperty.SetValue(result, property.GetValue(propertyValues));
            }
        }

        return result;
    }

    private static TypeBuilder CreateTypeBuilder(string typeName, Type interfaceType)
    {
        var assemblyName = new AssemblyName(typeName);
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(typeName);
        var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);
        var propertyAttibutes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;

        var properties = interfaceType
            .GetInterfaces().SelectMany(s => s.GetProperties())
            .Union(interfaceType.GetProperties());
        foreach (var property in properties)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + property.Name, property.PropertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
            var getMethodBuilder = typeBuilder.DefineMethod("get_" + property.Name, propertyAttibutes, property.PropertyType, Type.EmptyTypes);
            var getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);
            var setMethodBuilder = typeBuilder.DefineMethod("set_" + property.Name, propertyAttibutes, null, new Type[] { property.PropertyType });
            var setIL = setMethodBuilder.GetILGenerator();
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

    private static bool CheckIfAnonymousType(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        // HACK: The only way to detect anonymous types right now.
        return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
            && type.IsGenericType && type.Name.Contains("AnonymousType")
            && type.Name.StartsWith("<>")
            && type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }
}
