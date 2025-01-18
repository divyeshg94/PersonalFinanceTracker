using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Model.Helpers
{
    public class DataMapper
    {
        static object _assinableCtorCacheLock = new object();
        static Dictionary<Type, ConstructorInfo> _assignableContructorCache = new Dictionary<Type, ConstructorInfo>();

        static object _readableTypePropertiesCacheeLock = new object();
        static Dictionary<Type, List<PropertyInfo>> _readableTypePropertiesCache = new Dictionary<Type, List<PropertyInfo>>();

        static object _writableTypePropertiesCacheLock = new object();
        static Dictionary<Type, List<PropertyInfo>> _writableTypePropertiesCache = new Dictionary<Type, List<PropertyInfo>>();

        public static K Map<T, K>(T source)
            where K : class, new()
        {
            if (source == null)
            {
                return default(K);
            }
            var destObj = new K();
            Map(source, destObj);
            return destObj;
        }

        public static void Map<T, K>(T source, K destObj) where K : class, new()
        {
            var sourceType = typeof(T);
            var destType = typeof(K);
            var sourceProps = GetReadableProperties(sourceType);
            var destProps = GetWritableProperties(destType);
            foreach (var destProp in destProps)
            {
                var sourceProp = sourceProps.FirstOrDefault(sp => sp.Name == destProp.Name);
                if (sourceProp == null || source == null)
                    continue;
                object sourceVal = sourceProp.GetValue(source);
                if (sourceVal == null)
                    continue;
                if (IsSimpleType(destProp.PropertyType))
                {
                    var sourcePropType = sourceProp.PropertyType;
                    if (sourcePropType == typeof(DateTime)
                        && (DateTime)sourceVal == DateTime.MinValue)
                    {
                        destProp.SetValue(destObj, SqlDateTime.MinValue.Value);
                    }
                    else
                    {
                        try
                        {
                            // sometimes like in BillingSettings, sourceVal is in string format irrespective of the destination property type
                            if (destProp.PropertyType != typeof(string) && sourceVal is string)
                            {
                                var destVal = GetPropertyValue(destProp.PropertyType, sourceVal);
                                if (destVal != null)
                                {
                                    bool isAssignable = destProp.PropertyType.IsAssignableFrom(destVal.GetType());
                                    if (isAssignable)
                                    {
                                        destProp.SetValue(destObj, destVal);
                                    }
                                }
                            }
                            else
                            {
                                destProp.SetValue(destObj, sourceVal);
                            }
                        }
                        catch (Exception)
                        {
                            var destVal = GetPropertyValue(destProp.PropertyType, sourceVal);
                            if (destVal != null)
                            {
                                bool isAssignable = destProp.GetType().IsAssignableFrom(destVal.GetType());
                                if (isAssignable)
                                {
                                    destProp.SetValue(destObj, destVal);
                                }
                            }
                        }
                    }
                }
                else if (IsNullableSimpleType(destProp.PropertyType))
                {
                    var genericUnderlyingType = destProp.PropertyType.GenericTypeArguments.FirstOrDefault();
                    var destVal = GetPropertyValue(genericUnderlyingType, sourceVal);
                    if (destVal != null)
                    {
                        destProp.SetValue(destObj, destVal);
                    }
                }
                else
                {
                    // try the best and ignore
                    try
                    {
                        if (sourceVal != null)
                        {
                            var destPropType = destProp.PropertyType;
                            var sourceValueType = sourceVal.GetType();
                            bool isAssignable = destPropType.IsAssignableFrom(sourceValueType);
                            if (isAssignable)
                            {
                                destProp.SetValue(destObj, sourceVal);
                                continue;
                            }
                            var rCtor = GetAssignableCtor(destPropType, sourceValueType);
                            if (rCtor != null)
                            {
                                var destVal = rCtor.Invoke(new[] { sourceVal });
                                destProp.SetValue(destObj, destVal);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private static ConstructorInfo GetAssignableCtor(Type destPropType, Type sourceValueType)
        {
            if (!_assignableContructorCache.ContainsKey(destPropType))
            {
                lock (_assinableCtorCacheLock)
                {
                    if (!_assignableContructorCache.ContainsKey(destPropType))
                    {
                        var ctors = destPropType.GetConstructors();
                        ConstructorInfo rCtor = null;
                        foreach (var ctor in ctors)
                        {
                            var cParams = ctor.GetParameters();
                            if (cParams.Length == 1)
                            {
                                var cParamType = cParams[0].ParameterType;
                                var isAssignable = cParamType.IsAssignableFrom(sourceValueType);
                                if (isAssignable)
                                {
                                    rCtor = ctor;
                                }
                            }
                        }
                        _assignableContructorCache.Add(destPropType, rCtor);
                    }
                }
            }
            return _assignableContructorCache[destPropType];
        }

        private static List<PropertyInfo> GetReadableProperties(Type type)
        {
            if (!_readableTypePropertiesCache.ContainsKey(type))
            {
                lock (_readableTypePropertiesCacheeLock)
                {
                    if (!_readableTypePropertiesCache.ContainsKey(type))
                    {
                        var props = type.GetPublicProperties().Where(p => p.CanRead).ToList();
                        _readableTypePropertiesCache.Add(type, props);
                    }
                }
            }
            return _readableTypePropertiesCache[type];
        }

        private static List<PropertyInfo> GetWritableProperties(Type type)
        {
            if (!_writableTypePropertiesCache.ContainsKey(type))
            {
                lock (_writableTypePropertiesCacheLock)
                {
                    if (!_writableTypePropertiesCache.ContainsKey(type))
                    {
                        var props = type.GetPublicProperties().Where(p => p.CanWrite).ToList();
                        _writableTypePropertiesCache.Add(type, props);
                    }
                }
            }
            return _writableTypePropertiesCache[type];
        }


        private static bool IsSimpleType(Type propertyType)
        {
            var simpleton = new Type[] { typeof(DateTime), typeof(string), typeof(Guid), typeof(TimeSpan), typeof(Decimal) };
            if (propertyType.IsPrimitive)
            {
                return true;
            }
            if (propertyType.IsEnum)
            {
                return true;
            }
            if (simpleton.Contains(propertyType))
            {
                return true;
            }
            return false;
        }

        private static bool IsNullableSimpleType(Type propertyType)
        {
            var simpleton = new Type[] { typeof(DateTime?), typeof(Guid?), typeof(TimeSpan?), typeof(Decimal?), typeof(Int32?), typeof(Int64?), typeof(short?) };
            return simpleton.Contains(propertyType);
        }

        public static JsonParameters GetInputProperties(object destObj, Type excludePropsDefinedInType)
        {
            var excludedTypeProps = GetWritableProperties(excludePropsDefinedInType);
            //var excludedTypeProps = excludePropsDefinedInType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite);
            var excludedPropNames = excludedTypeProps.Select(p => p.Name);
            return GetInputProperties(destObj, excludedPropNames.ToArray());
        }

        public static JsonParameters GetInputProperties(object destObj, params string[] excludedProps)
        {
            Type type = destObj.GetType();
            JsonParameters inputParameters = new JsonParameters();
            var destProps = GetWritableProperties(type);
            //var destProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite);
            foreach (var destProp in destProps)
            {
                if (!excludedProps.Contains(destProp.Name))
                {
                    inputParameters.Add(destProp.Name, destProp.GetValue(destObj));
                }
            }
            return inputParameters;
        }

        public static void FillProperties(object destObj, IDictionary<string, string> inputParameters)
        {
            var destType = destObj.GetType();
            var destProps = GetWritableProperties(destType);
            //var destProps = destType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite);
            foreach (var destProp in destProps)
            {
                if (!inputParameters.ContainsKey(destProp.Name))
                    continue;

                var sourceValue = inputParameters[destProp.Name];
                var propValue = GetPropertyValue(destProp.PropertyType, sourceValue);
                if (propValue != null)
                {
                    destProp.SetValue(destObj, propValue);
                }
            }
        }


        public static object GetPropertyValue(Type propertyType, object sourceValue)
        {
            if (sourceValue == null)
            {
                return null;
            }
            var sourceValStr = sourceValue as string;
            if (propertyType == typeof(string))
            {
                if (sourceValStr == null)
                {
                    return sourceValue?.ToString();
                }
                return sourceValue;
            }

            if (sourceValStr != null)
            {
                if (propertyType.IsEnum)
                {
                    return Enum.Parse(propertyType, sourceValStr);
                }

                if (propertyType == typeof(bool))
                {
                    var boolstring = sourceValStr;
                    if (boolstring != null)
                    {
                        try
                        {
                            var r = false;
                            if (bool.TryParse(boolstring, out r))
                            {
                                return r;
                            }
                            if (!string.IsNullOrEmpty(boolstring))
                            {
                                return boolstring == "1";
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                try
                {
                    if (propertyType == typeof(Guid))
                    {
                        Guid guid;
                        if (Guid.TryParse(sourceValue?.ToString(), out guid))
                        {
                            return guid;
                        }
                    }
                }
                catch
                {

                }

                if (propertyType.Name == "Nullable`1")
                {
                    var genericUnderlyingType = propertyType.GenericTypeArguments.FirstOrDefault();
                    return GetPropertyValue(genericUnderlyingType, sourceValue);
                }
                try
                {
                    return Convert.ChangeType(sourceValue, propertyType);
                }
                catch
                {
                    // ignored
                }
            }
            return sourceValue;
        }

    }
}
