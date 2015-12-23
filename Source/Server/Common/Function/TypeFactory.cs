using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Insight.WS.Server.Common
{

    public class TypeFactory
    {

        /// <summary>
        /// 获取动态自定义类型
        /// </summary>
        /// <param name="itemList"></param>
        /// <returns></returns>
        public static Type GetUserType(params PropertyItem[] itemList)
        {
            var builder = CreateTypeBuilder("MyDynamicAssembly", "MyModule", "MyType");
            foreach (var item in itemList)
            {
                CreateAutoImplementedProperty(builder, item.Name, item.Type);
            }

            var resultType = builder.CreateType();
            return resultType;
        }

        /// <summary>
        /// 组装动态类型
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="moduleName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static TypeBuilder CreateTypeBuilder(string assemblyName, string moduleName, string typeName)
        {
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var typeBuilder = assemblyBuilder.DefineDynamicModule(moduleName).DefineType(typeName, TypeAttributes.Public);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            return typeBuilder;
        }

        /// <summary>
        /// 创建类型属性
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        private static void CreateAutoImplementedProperty(TypeBuilder builder, string propertyName, Type propertyType)
        {
            const string PrivateFieldPrefix = "m_";
            const string GetterPrefix = "get_";
            const string SetterPrefix = "set_";

            // 定义字段.
            var fieldBuilder = builder.DefineField(string.Concat(PrivateFieldPrefix, propertyName), propertyType, FieldAttributes.Private);

            // 定义属性
            var propertyBuilder = builder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            // 属性的getter和setter的特性
            const MethodAttributes propertyMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            // 定义getter方法
            var getterMethod = builder.DefineMethod(string.Concat(GetterPrefix, propertyName), propertyMethodAttributes, propertyType, Type.EmptyTypes);

            var getterILCode = getterMethod.GetILGenerator();
            getterILCode.Emit(OpCodes.Ldarg_0);
            getterILCode.Emit(OpCodes.Ldfld, fieldBuilder);
            getterILCode.Emit(OpCodes.Ret);

            // 定义setter方法
            var setterMethod = builder.DefineMethod(string.Concat(SetterPrefix, propertyName), propertyMethodAttributes, null, new[] { propertyType });

            var setterILCode = setterMethod.GetILGenerator();
            setterILCode.Emit(OpCodes.Ldarg_0);
            setterILCode.Emit(OpCodes.Ldarg_1);
            setterILCode.Emit(OpCodes.Stfld, fieldBuilder);
            setterILCode.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethod);
            propertyBuilder.SetSetMethod(setterMethod);
        }
    }

    public class PropertyItem
    {

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { set; get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public PropertyItem(string name, Type type)
        {
            Name = name;
            Type = type;
        }

    }

}
