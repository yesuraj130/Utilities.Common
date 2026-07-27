using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities.Common.Extensions
{
    public static class ReflectionExt
    {
        private static readonly Dictionary<Type, IReadOnlyDictionary<string, PropertyInfoExt>> PropertyInfos = new Dictionary<Type, IReadOnlyDictionary<string, PropertyInfoExt>>();
        public static bool CopyValueTypePropertiesByReflection<TDest, TSource>(this TDest destination, TSource source, string[] IgnorableProperties = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            bool Result = false;

            var DestinationProperties = GetProperties(destination.GetType());
            var SourceProperties = GetProperties(source.GetType());

            if (DestinationProperties is null || SourceProperties is null) return false;

            var CheckIgnorableProperties = false;
            if (IgnorableProperties != null && IgnorableProperties.Any(x => !string.IsNullOrEmpty(x))) CheckIgnorableProperties = true;

            foreach (var DestinationProperty in DestinationProperties)
            {
                if (DestinationProperty.Value.Setter == null) continue;

                if (CheckIgnorableProperties)
                {
                    if (IgnorableProperties.Any(x => DestinationProperty.Value.PropertyInfo.Name == x)) continue;
                }

                if (SourceProperties.TryGetValue(DestinationProperty.Key, out var SourceProperty) && DestinationProperty.Value.PropertyInfo.PropertyType == SourceProperty.PropertyInfo.PropertyType)
                {
                    var DestinationValue = DestinationProperty.Value.Getter(destination);
                    var SourceValue = SourceProperty.Getter(source);
                    if (DestinationValue == null || SourceValue == null)
                    {
                        if (DestinationValue != SourceValue)
                        {
                            DestinationProperty.Value.Setter(destination, SourceValue);
                            Result = true;
                        }

                    }
                    else if (!DestinationValue.Equals(SourceValue))
                    {
                        DestinationProperty.Value.Setter(destination, SourceValue);
                        Result = true;
                    }
                }
            }
            return Result;
        }

        public static string GetPropertyChangesByReflection<TDest, TSource>(this TDest destination, TSource source, string[] IgnorableProperties = null)
        {
            StringBuilder Result = new StringBuilder();
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var DestinationProperties = GetProperties(destination.GetType());
            var SourceProperties = GetProperties(source.GetType());

            if (DestinationProperties is null || SourceProperties is null) return "Error reading property changes";

            foreach (var DestinationProperty in DestinationProperties)
            {
                if (IgnorableProperties is not null && IgnorableProperties.Any(x => x == DestinationProperty.Key)) continue;
                if (DestinationProperty.Value.Setter == null) continue;
                if (SourceProperties.TryGetValue(DestinationProperty.Key, out var SourceProperty) && DestinationProperty.Value.PropertyInfo.PropertyType == SourceProperty.PropertyInfo.PropertyType)
                {
                    bool IsChanged = false;
                    var DestinationValue = DestinationProperty.Value.Getter(destination);
                    var SourceValue = SourceProperty.Getter(source);
                    if (DestinationValue == null || SourceValue == null)
                    {
                        if (DestinationValue != SourceValue)
                        {
                            IsChanged = true;
                        }

                    }
                    else if (!DestinationValue.Equals(SourceValue))
                    {
                        IsChanged = true;
                    }
                    if (IsChanged)
                    {
                        string New;
                        string Existing;
                        if (SourceValue == null) New = string.Empty;
                        else New = SourceValue.ToString().Trim();
                        if (DestinationValue == null) Existing = string.Empty;
                        else Existing = DestinationValue.ToString().Trim();

                        if (string.IsNullOrWhiteSpace(Existing) && !string.IsNullOrWhiteSpace(New))
                        {
                            Result.AppendLine("Added: \"" + New + "\"" + " at " + DestinationProperty.Key);
                        }
                        else if (string.IsNullOrWhiteSpace(New) && !string.IsNullOrWhiteSpace(Existing))
                        {
                            Result.AppendLine("Removed: \"" + Existing + "\"" + " at " + DestinationProperty.Key);
                        }
                        else if (!string.IsNullOrWhiteSpace(New) && !string.IsNullOrWhiteSpace(Existing))
                        {
                            Result.AppendLine("Changed: \"" + Existing + "\" to \"" + New + "\" " + " at " + DestinationProperty.Key);
                        }
                    }
                }

            }
            return Result.ToString().Trim();
        }
        public static string GetPropertyValuesByReflection<TDest>(this TDest destination)
        {
            StringBuilder Result = new StringBuilder();
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var DestinationProperties = GetProperties(destination.GetType());

            if (DestinationProperties is null) return "Error reading property changes";

            foreach (var DestinationProperty in DestinationProperties)
            {
                if (DestinationProperty.Value.Setter == null) continue;
                var DestinationValue = DestinationProperty.Value.Getter(destination);
                string Existing;
                if (DestinationValue == null) Existing = "Null";
                else Existing = DestinationValue.ToString().Trim();
                Result.AppendLine($"{DestinationProperty.Key}: \"{Existing}\"");
            }
            return Result.ToString().Trim();
        }
        public static bool CompareValueTypePropertiesByReflection<TDest, TSource>(this TDest Actual, TSource Comparer, string[] IgnorableProperties = null)
        {
            if (Actual == null) throw new ArgumentNullException(nameof(Actual));
            if (Comparer == null) throw new ArgumentNullException(nameof(Comparer));

            var ActualProperties = GetProperties(Actual.GetType());
            var ComparerProperties = GetProperties(Comparer.GetType());

            if (ActualProperties is null || ComparerProperties is null) return false;

            var CheckIgnorableProperties = false;
            if (IgnorableProperties != null && IgnorableProperties.Any(x => !string.IsNullOrEmpty(x))) CheckIgnorableProperties = true;

            foreach (var ActualProperty in ActualProperties)
            {
                if (ActualProperty.Value.Setter == null) continue;

                if (CheckIgnorableProperties)
                {
                    if (IgnorableProperties.Any(x => ActualProperty.Value.PropertyInfo.Name == x)) continue;
                }

                if (ComparerProperties.TryGetValue(ActualProperty.Key, out var ComparerProperty) && ActualProperty.Value.PropertyInfo.PropertyType == ComparerProperty.PropertyInfo.PropertyType)
                {
                    var ActualValue = ActualProperty.Value.Getter(Actual);
                    var ComparerValue = ComparerProperty.Getter(Comparer);
                    if (ActualValue == null || ComparerValue == null)
                    {
                        if (ActualValue != ComparerValue)
                        {
                            return false;
                        }

                    }
                    else if (!ActualValue.Equals(ComparerValue))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //private static class ILHelper
        //{
        //    public static Func<object, object> CreateGetter(MethodInfo method)
        //    {
        //        if (method == null) return null;
        //        var dm = new DynamicMethod(method.Name, typeof(object), new Type[] { typeof(object) }, method.DeclaringType, true);
        //        InjectCodeGetter(dm, method);
        //        return (Func<object, object>)dm.CreateDelegate(typeof(Func<object, object>));
        //    }
        //    public static Action<object, object> CreateSetter(MethodInfo method)
        //    {
        //        if (method == null) return null;
        //        var dm = new DynamicMethod(method.Name, typeof(void), new Type[] { typeof(object), typeof(object) }, method.DeclaringType, true);
        //        InjectCodeSetter(dm, method);
        //        return (Action<object, object>)dm.CreateDelegate(typeof(Action<object, object>));
        //    }
        //    private static void InjectCodeGetter(DynamicMethod dm, MethodInfo method)
        //    {
        //        var il = dm.GetILGenerator();

        //        if (!method.IsStatic)
        //        {
        //            il.Emit(OpCodes.Ldarg_0);
        //            il.Emit(OpCodes.Unbox_Any, method.DeclaringType);
        //        }
        //        var parameters = method.GetParameters();
        //        for (int i = 0; i < parameters.Length; i++)
        //        {
        //            il.Emit(OpCodes.Ldarg_1);
        //            il.Emit(OpCodes.Ldc_I4, i);
        //            il.Emit(OpCodes.Ldelem_Ref);
        //            il.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
        //        }
        //        il.EmitCall(method.IsStatic || method.DeclaringType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, method, null);
        //        if (method.ReturnType == null || method.ReturnType == typeof(void))
        //        {
        //            il.Emit(OpCodes.Ldnull);
        //        }
        //        else if (method.ReturnType.IsValueType)
        //        {
        //            il.Emit(OpCodes.Box, method.ReturnType);
        //        }
        //        il.Emit(OpCodes.Ret);
        //    }
        //    private static void InjectCodeSetter(DynamicMethod dm, MethodInfo method)
        //    {
        //        var il = dm.GetILGenerator();

        //        if (!method.IsStatic)
        //        {
        //            il.Emit(OpCodes.Ldarg_0);
        //            il.Emit(OpCodes.Unbox_Any, method.DeclaringType);
        //        }
        //        var parameters = method.GetParameters();
        //        for (int i = 0; i < parameters.Length; i++)
        //        {
        //            il.Emit(OpCodes.Ldarg_1);
        //            il.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
        //        }
        //        il.EmitCall(method.IsStatic || method.DeclaringType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, method, null);
        //        il.Emit(OpCodes.Ret);
        //    }
        //}

        public static bool IsList(this object o)
        {
            if (o is null) return false;
            return o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsDictionary(this object o)
        {
            if (o is null) return false;
            return o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }

        public static bool IsGenericTypeOf(this object o, Type GenericType)
        {
            if (o is null) return false;
            return o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(GenericType);
        }

        #region Private Methods
        public static IReadOnlyDictionary<string, PropertyInfoExt> GetProperties(Type ObjectType)
        {
            if (ObjectType is null) return null;

            if (PropertyInfos.TryGetValue(ObjectType, out var Match)) return Match;

            List<PropertyInfoExt> TempPropertiesList = new List<PropertyInfoExt>();
            foreach (PropertyInfo targetProperty in ObjectType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (targetProperty is not null && targetProperty.Name is not null && (targetProperty.PropertyType.IsValueType || targetProperty.PropertyType == typeof(string)))
                {
                    TempPropertiesList.Add(new PropertyInfoExt(targetProperty,
                        targetProperty.CanRead && targetProperty.GetGetMethod(false) is not null ? (obj) => targetProperty.GetGetMethod(false).CreateDelegate(System.Linq.Expressions.Expression.GetFuncType(ObjectType, targetProperty.PropertyType)).DynamicInvoke(obj) : null,
                        targetProperty.CanWrite && targetProperty.GetSetMethod(false) is not null ? (obj, value) => targetProperty.GetSetMethod(false).CreateDelegate(System.Linq.Expressions.Expression.GetActionType(ObjectType, targetProperty.PropertyType)).DynamicInvoke(obj, value) : null));
                }
            }
            var Result = TempPropertiesList.ToDictionary(x => x.PropertyInfo.Name, x => x);
            if (!PropertyInfos.ContainsKey(ObjectType)) try { PropertyInfos.Add(ObjectType, Result); } catch { }
            return Result;
        }
        #endregion

    }

    public class PropertyInfoExt
    {
        public PropertyInfo PropertyInfo { get; }
        public Func<object, object> Getter { get; }
        public Action<object, object> Setter { get; }
        //public bool CanRead { get; }
        //public bool CanWrite { get; }
        //public bool IsValueType { get; }

        public PropertyInfoExt(PropertyInfo PropertyInfo, Func<object, object> Getter, Action<object, object> Setter)
        {
            this.PropertyInfo = PropertyInfo;
            this.Getter = Getter;
            this.Setter = Setter;
            //this.CanRead = Getter is not null;
            //this.CanWrite = Setter is not null;
        }
    }
}