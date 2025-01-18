using System.Reflection;

namespace PersonalFinanceTracker.Model.Helpers
{
    public static class TypeExtensions
    {
        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
        {
            if (!type.IsInterface)
                return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            return (new Type[] { type })
                   .Concat(type.GetInterfaces())
                   .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public));
        }

        public static IEnumerable<Type> GetImplementationClasses(this Type interfaceType)
        {
            IEnumerable<Type> implTypes = interfaceType.Assembly.GetTypes().Where(
                                                t => interfaceType.IsAssignableFrom(t)
                                                    && t.IsClass);
            return implTypes;
        }
    }
}
