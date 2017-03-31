using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.Linq.Expressions;
using System.Reflection.Emit;
using fastJSON;

namespace NetThemis
{
    public class SerializerOption
    {
        private static SerializerOption _default = new SerializerOption();
        public static SerializerOption Define
        {
            get { return _default; }
        }

        private static bool s_initialized;


        public  HashSet<NodeItem> _DefineNodeMap = new HashSet<NodeItem>();
        public HashSet<TypeItem> _DefineDataType = new HashSet<TypeItem>();

        public SerializerOption()
        {
            Initialize();
        }

        private  void Initialize()
        {
            if (s_initialized)
                throw new InvalidOperationException("NetSerializer already initialized");


            var primitives = new[]
            {
                typeof(bool), //50
                typeof(byte), //51 
                typeof(sbyte), //52
                typeof(char), //53
                typeof(ushort), //54 
                typeof(short), //56
                typeof(uint), //57
                typeof(int), //58
                typeof(ulong), //59 
                typeof(long), //60
                typeof(float), //61 
                typeof(double), //62
                typeof(string) //63
            };

            var typeSet = new HashSet<Type>(primitives);

            typeSet = new HashSet<Type>(typeSet.OrderBy(t => t.FullName, StringComparer.Ordinal));

            var defineDataType = new HashSet<TypeItem>();

            foreach (var type in primitives)
            {
                var item = new TypeItem
                {
                    DataType = type,
                    Tid = defineDataType.Count + Config.SYSTEM_TYPE_ID,
                    Name = type.Name
                };
                defineDataType.Add(item);
            }

            var sb = new StringBuilder();
            foreach (var type in defineDataType)
            {
                sb.AppendLine(type.Name + "=" + type.Tid);
            }
            var str = sb.ToString();

            foreach (var typeItem in defineDataType)
            {
                _DefineDataType.Add(typeItem);
            }
            s_initialized = true;

            
        }

        public static Func<object, object[]> BuildArrayConvert(Type type)
        {
            //if (type == typeof(int[]))
            //{
            //    return FunInput;
            //}
            var assemblyName = new AssemblyName("DLL" + type.Name);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run, AppDomain.CurrentDomain.BaseDirectory);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            TypeBuilder builder = moduleBuilder.DefineType("Test", TypeAttributes.Public);

            var methodBuilder = builder.DefineMethod("DynamicCreate", MethodAttributes.Public, typeof(object[]), new Type[] { typeof(object) });


            
            ILGenerator il = methodBuilder.GetILGenerator();
          
      
            il.DeclareLocal(typeof(object[]));
            il.DeclareLocal(type);
            il.DeclareLocal(typeof(int));


            Label targetInstruction1 = il.DefineLabel();
            Label targetInstruction2 = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Isinst,type);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Newarr,typeof(object));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Br_S, targetInstruction2);//Lable

            il.MarkLabel(targetInstruction1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldelem_I4);

            il.Emit(OpCodes.Box,type.GetElementType());
            il.Emit(OpCodes.Stelem_Ref);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_2);

            il.MarkLabel(targetInstruction2);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Blt_S, targetInstruction1);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

          
            {
                var t = builder.CreateType();
                //assemblyBuilder.Save(assemblyName.Name + ".dll");

                var obj = Activator.CreateInstance(t);
              
                var minfo = obj.GetType().GetMethod("DynamicCreate");
                //return methodBuilder.CreateDelegate(typeof(Func<object, object[]>)) as Func<object, object[]>;
                //Delegate.CreateDelegate(typeof(Func<object, object[]>),)
                return Delegate.CreateDelegate(typeof(Func<object, object[]>), obj, minfo) as Func<object, object[]>;
            }
        }

        public static Func<object, object[]> BuildListConvert(Type type)
        {
            //if (type == typeof(int[]))
            //{
            //    return FunInput;
            //}
            var assemblyName = new AssemblyName("DLL" + type.Name);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run, AppDomain.CurrentDomain.BaseDirectory);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);//, assemblyName.Name + ".dll");

            TypeBuilder builder = moduleBuilder.DefineType("Test", TypeAttributes.Public);

            var methodBuilder = builder.DefineMethod("DynamicCreate", MethodAttributes.Public, typeof(object[]), new Type[] { typeof(object) });



            ILGenerator il = methodBuilder.GetILGenerator();


            il.DeclareLocal(typeof(object[]));
            il.DeclareLocal(type);
            il.DeclareLocal(typeof(int));


            Label targetInstruction1 = il.DefineLabel();
            Label targetInstruction2 = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Isinst, type);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldloc_1);
            var p = type.GetProperty("Count").GetMethod;
            il.Emit(OpCodes.Callvirt,p);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Br_S, targetInstruction2);//Lable

            il.MarkLabel(targetInstruction1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_2);
            var px = type.GetMethod("get_Item");
            il.Emit(OpCodes.Callvirt, px);
            il.Emit(OpCodes.Stelem_Ref);

            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_2);

            il.MarkLabel(targetInstruction2);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Blt_S, targetInstruction1);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);


            {
                var t = builder.CreateType();
                //assemblyBuilder.Save(assemblyName.Name + ".dll");

                var obj = Activator.CreateInstance(t);

                var minfo = obj.GetType().GetMethod("DynamicCreate");
                //return methodBuilder.CreateDelegate(typeof(Func<object, object[]>)) as Func<object, object[]>;
                //Delegate.CreateDelegate(typeof(Func<object, object[]>),)
                return Delegate.CreateDelegate(typeof(Func<object, object[]>), obj, minfo) as Func<object, object[]>;
            }
        }

        public static Func<List<object>,object> BuildArrayConvertBack(Type type,bool save=false)
        {
            var assemblyName = new AssemblyName("DLL2" + type.Name);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, save ? AssemblyBuilderAccess.RunAndSave :  AssemblyBuilderAccess.Run, AppDomain.CurrentDomain.BaseDirectory);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            if (save)
            {
                moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name + "_s", assemblyName.Name + ".dll");
               
            }

            TypeBuilder builder = moduleBuilder.DefineType("Test", TypeAttributes.Public);

            var methodBuilder = builder.DefineMethod("DynamicCreate", MethodAttributes.Public, typeof(object), new Type[] { typeof(List<object>) });



            ILGenerator il = methodBuilder.GetILGenerator();

            var etype = type.GetElementType();

            il.DeclareLocal(type);
            il.DeclareLocal(etype);


            Label targetInstruction1 = il.DefineLabel();
            Label targetInstruction2 = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_1);
        
            var p = typeof(List<object>).GetProperty("Count").GetMethod;
            il.Emit(OpCodes.Callvirt, p);
            il.Emit(OpCodes.Newarr, etype);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Br_S, targetInstruction2);//Lable

            il.MarkLabel(targetInstruction1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc_1);
            var px = typeof(List<object>).GetMethod("get_Item");
            il.Emit(OpCodes.Callvirt, px);
            il.Emit(OpCodes.Unbox_Any, etype);
            il.Emit(OpCodes.Stelem_I4);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_1);

            il.MarkLabel(targetInstruction2);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_0);

            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Blt_S, targetInstruction1);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);


            {
                var t = builder.CreateType();
                if(save)
                assemblyBuilder.Save(assemblyName.Name + ".dll");

                var obj = Activator.CreateInstance(t);

                var minfo = obj.GetType().GetMethod("DynamicCreate");
                //return methodBuilder.CreateDelegate(typeof(Func<object, object[]>)) as Func<object, object[]>;
                //Delegate.CreateDelegate(typeof(Func<object, object[]>),)
                return Delegate.CreateDelegate(typeof(Func<List<object>, object>), obj, minfo) as Func<List<object>, object>;
            }
        }

        public static Func<List<object>, object> BuildListConvertBack(Type type, bool save = false)
        {
            var assemblyName = new AssemblyName("DLL2" + type.Name);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, save ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run, AppDomain.CurrentDomain.BaseDirectory);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            if (save)
            {
                moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name + "_s", assemblyName.Name + ".dll");

            }

            TypeBuilder builder = moduleBuilder.DefineType("Test", TypeAttributes.Public);

            var methodBuilder = builder.DefineMethod("DynamicCreate", MethodAttributes.Public, typeof(object), new Type[] { typeof(List<object>) });



            ILGenerator il = methodBuilder.GetILGenerator();

            var etype = type.GetGenericArguments()[0];

            il.DeclareLocal(type);
            il.DeclareLocal(etype);


            Label targetInstruction1 = il.DefineLabel();
            Label targetInstruction2 = il.DefineLabel();

            il.Emit(OpCodes.Newobj,type.GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Br_S, targetInstruction2);

            il.MarkLabel(targetInstruction1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Callvirt, typeof(List<object>).GetMethod("get_Item"));
            il.Emit(OpCodes.Unbox_Any, etype);
            il.Emit(OpCodes.Callvirt, type.GetMethod("Add"));
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_1);

            il.MarkLabel(targetInstruction2);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, typeof(List<object>).GetProperty("Count").GetMethod);
            il.Emit(OpCodes.Blt_S, targetInstruction1);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            {
                var t = builder.CreateType();
                if (save)
                    assemblyBuilder.Save(assemblyName.Name + ".dll");

                var obj = Activator.CreateInstance(t);

                var minfo = obj.GetType().GetMethod("DynamicCreate");
                //return methodBuilder.CreateDelegate(typeof(Func<object, object[]>)) as Func<object, object[]>;
                //Delegate.CreateDelegate(typeof(Func<object, object[]>),)
                return Delegate.CreateDelegate(typeof(Func< List<object>,object>), obj, minfo) as Func< List<object>,object>;
            }
        }

     
       internal  static Func<object> FastCreateInstanceBuild(Type objtype)
        {
            try
            {
                Func<object> c = null;
                if (objtype.IsClass)
                {
                    DynamicMethod dynMethod = new DynamicMethod("_", objtype, null);
                    ILGenerator ilGen = dynMethod.GetILGenerator();
                    ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
                    ilGen.Emit(OpCodes.Ret);
                    c = (Func<object>)dynMethod.CreateDelegate(typeof(Func<object>));
                    return c;
                }
                else // structs
                {
                    DynamicMethod dynMethod = new DynamicMethod("_", typeof(object), null);
                    ILGenerator ilGen = dynMethod.GetILGenerator();
                    var lv = ilGen.DeclareLocal(objtype);
                    ilGen.Emit(OpCodes.Ldloca_S, lv);
                    ilGen.Emit(OpCodes.Initobj, objtype);
                    ilGen.Emit(OpCodes.Ldloc_0);
                    ilGen.Emit(OpCodes.Box, objtype);
                    ilGen.Emit(OpCodes.Ret);
                    c = (Func<object>)dynMethod.CreateDelegate(typeof(Func<object>));
                    return c;
                }
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format("Failed to fast create instance for type '{0}' from assembly '{1}'",
                    objtype.FullName, objtype.AssemblyQualifiedName), exc);
            }
        }

        /*
         
  
        public  object FunOutPut1(List<object> input)
        {
            var result = new List<int>();

            for (int i = 0; i < input.Count; i++)
            {
                result.Add((int)input[i]);
            }

            return result;
        }

        public  object FunOutPut(List<object> input)
        {
            var result = new int[input.Count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (int) input[i];
            }

            return result;
        }

        public  object[] FunInput(object input)
        {
            object[] result;

            var item = (input as int[]);
            result = new object[item.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = item[i];
            }

            return result;
        }

        public  object[] FunInput1(object input)
        {
            object[] result;

            var item = (input as IList<string>);
            result = new object[item.Count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = item[i];
            }

            return result;
        }
        */
        public SchemaDefine BuildTypeSubSchema(string name, Type ptype)
        {
            var pdefine = _DefineNodeMap.FirstOrDefault(r => r.Name == name && r.DataType == ptype);

            var tdefine = _DefineDataType.FirstOrDefault(r => r.DataType == ptype);

            var define = new SchemaDefine()
            {
                SrcType = ptype,
                GetType = GetType(ptype),
              
            };

            if (pdefine != null)
            {
                define.HeadItem = pdefine;
                if (tdefine == null)
                {
                    define.HeadItem = null;
                }
                define.HeadDefine = pdefine.Tid;
                if (ptype.IsArray || ptype.GetInterface("IList", true) != null)
                {
                    define.IsArray = true;
                    var subtype = GetItemType(ptype);

                    define.SubDefine = BuildTypeSubSchema(null, subtype);

                    if (ptype.IsArray)
                    {
                        define.arrayConvert = BuildArrayConvert(ptype);
                        define.arrayConvertBack = BuildArrayConvertBack(ptype);
                    }
                    else
                    {
                        define.arrayConvert = BuildListConvert(ptype);
                        define.arrayConvertBack = BuildListConvertBack(ptype);
                        define.IsList = true;
                    }
                }
            }
            else
            {
                define.HeadItem = tdefine;

                if (tdefine == null)
                {
                    if (ptype.IsArray || ptype.GetInterface("IList", true) != null)
                    {
                        define.IsArray = true;
                        var subtype = GetItemType(ptype);

                        define.SubDefine = BuildTypeSubSchema(null, subtype);


                        if (ptype.IsArray)
                        {
                            define.arrayConvert = BuildArrayConvert(ptype);
                            define.arrayConvertBack = BuildArrayConvertBack(ptype);
                        }
                        else
                        {
                            define.arrayConvert = BuildListConvert(ptype);
                            define.arrayConvertBack = BuildListConvertBack(ptype);
                            define.IsList = true;
                        }
                    }
                }
            }

            return define;
        }

        public void AddType(Type type, Func<string, Type, Type> func = null)
        {



            foreach (var map in GetProperties(type))
            {
                var ptype = map.PropertyType;
                if (ptype.IsEnum)
                    ptype = Enum.GetUnderlyingType(ptype);
                if (func != null)
                {
                    ptype = func(map.Name, ptype);
                }
                if (!_DefineNodeMap.Any(r => r.Type == ptype && r.Name == map.Name))
                {
                    var typeDate = _DefineDataType.FirstOrDefault(r => r.DataType == ptype);


                    _DefineNodeMap.Add(new NodeItem
                    {
                        DataHead = typeDate == null ? (int)DataHead.Dic : typeDate.Tid,
                        DataType = ptype,
                        Tid = _DefineNodeMap.Count + Config.NODE_DEFINE_ID,
                        Name = map.Name
                    });

                    if (ptype != null && !ptype.IsValueType)
                    {
                        _DefineNodeMap.Add(new NodeItem
                        {
                            DataHead = (int)DataHead.Null,
                            DataType = null,
                            Tid = _DefineNodeMap.Count + Config.NODE_DEFINE_ID,
                            Name = map.Name
                        });
                    }
                }
            }

        
         

        }


        public  Type GetType(Type type)
        {
           
            Type resultType = null;
       
            if (type.IsEnum)
            {
                resultType = Enum.GetUnderlyingType(type);
            }
            else if (type == typeof(Type))
            {
                resultType = typeof(string);
            }
            else
            {
                resultType = type;
            }
         
            return resultType;
        }


  
        internal  Type GetItemType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }
            throw new NotImplementedException();
        }


        private  void CollectTypes(Type type, HashSet<Type> typeSet)
        {
            if (typeSet.Contains(type))
                return;

            if (type.IsAbstract)
                return;

            if (type.IsInterface)
                return;

            if (type.Name.StartsWith(typeof(void).Name))
                return;

            //if (!type.IsSerializable)
            //    throw new NotSupportedException(String.Format("Type {0} is not marked as Serializable", type.FullName));

            if (type.ContainsGenericParameters)
                return;

            typeSet.Add(type);

            if (type.IsArray)
            {
                CollectTypes(type.GetElementType(), typeSet);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var args = type.GetGenericArguments();

                // Dictionary<K,V> is stored as KeyValuePair<K,V>[]

                var arrayType = typeof(KeyValuePair<,>).MakeGenericType(args).MakeArrayType();

                CollectTypes(arrayType, typeSet);
            }
            else
            {
                var pinfos = GetProperties(type);

                foreach (var field in pinfos)
                    CollectTypes(field.PropertyType, typeSet);
            }
        }


        public  IEnumerable<PropertyInfo> GetProperties(Type type)
        {

            var pinfos = type.GetProperties()
                .Where(r => !r.IsDefined(typeof(XmlIgnoreAttribute),true))
                .OrderBy(f => f.Name, StringComparer.Ordinal);
            ;

            if (type.BaseType == null)
            {
                return pinfos;
            }
            var baseFields = GetProperties(type.BaseType);
            var newList = new List<PropertyInfo>();
            foreach (var propertyInfo in baseFields)
            {
                if (!pinfos.Any(r => r.Name == propertyInfo.Name))
                {
                    newList.Add(propertyInfo);
                }
            }
            return newList.Concat(pinfos).ToArray();
        }
    }
}
 