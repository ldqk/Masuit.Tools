using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace Masuit.Tools.Database
{
    internal class DataTableBuilder<T>
    {
        private static readonly MethodInfo GetValueMethod = typeof(DataRow).GetMethod("get_Item", new[]
        {
            typeof(int)
        });

        private static readonly MethodInfo IsDbNullMethod = typeof(DataRow).GetMethod("IsNull", new[]
        {
            typeof(int)
        });

        private delegate T Load(DataRow dataRecord);

        private Load _handler;

        private DataTableBuilder()
        {
        }

        public T Build(DataRow dataRecord)
        {
            return _handler(dataRecord);
        }

        public static DataTableBuilder<T> CreateBuilder(DataRow dataRecord)
        {
            DataTableBuilder<T> dynamicBuilder = new DataTableBuilder<T>();
            DynamicMethod method = new DynamicMethod("DynamicCreateEntity", typeof(T), new Type[]
            {
                typeof(DataRow)
            }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();
            LocalBuilder result = generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);
            for (int i = 0; i < dataRecord.ItemArray.Length; i++)
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(dataRecord.Table.Columns[i].ColumnName);
                Label endIfLabel = generator.DefineLabel();
                if (propertyInfo != null && propertyInfo.GetSetMethod() != null)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, IsDbNullMethod);
                    generator.Emit(OpCodes.Brtrue, endIfLabel);
                    generator.Emit(OpCodes.Ldloc, result);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, GetValueMethod);
                    generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                    generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                    generator.MarkLabel(endIfLabel);
                }
            }

            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);
            dynamicBuilder._handler = (Load)method.CreateDelegate(typeof(Load));
            return dynamicBuilder;
        }
    }
}