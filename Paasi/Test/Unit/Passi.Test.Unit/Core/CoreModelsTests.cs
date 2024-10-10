using System.Collections;
using System.Reflection;

namespace Passi.Test.Unit.Core
{
    public class CoreEntitiesTest
    {
        [Theory]

        [InlineData("Passi.Core", "Passi.Core.Application.Options")]
        [InlineData("Passi.Core", "Passi.Core.Domain.Entities")]
        public void CoreModels_Construct_OK(string assemblyName, params string[] namespaces)
        {
            Assembly assembly = Assembly.Load(assemblyName);
            List<Type> types = assembly
                .GetTypes()
                .Where(t => namespaces.Contains(t.Namespace))
                .ToList();
            foreach (Type type in types)
            {
                object? r;
                if (type.IsGenericType)
                {
                    Type constructedType = type.MakeGenericType(typeof(object));
                    r = Activator.CreateInstance(constructedType);
                }
                else
                {
                    r = Activator.CreateInstance(type);
                }
                PropertyInfo[] propertyInfo = r!.GetType().GetProperties();
                foreach (var property in propertyInfo)
                {
                    var _value = property.GetValue(r);
                    var _propertyType = property.PropertyType;
                    if (!_propertyType.IsAbstract)
                    {
                        if (_propertyType.IsGenericType && _propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            Assert.True(_value == null);
                        }
                        else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && !_value!.GetType().IsAssignableFrom(typeof(string)))
                        {
                            Assert.True(!((IEnumerable)_value!).GetEnumerator().MoveNext());
                        }
                        else if (_value != null && !_propertyType.IsValueType)
                        {
                            Assert.True(_value!.GetType().IsAssignableFrom(_propertyType));
                        }
                        else
                        {
                            var defValue = Activator.CreateInstance(_propertyType);
                            if (defValue == null)
                            {
                                Assert.Null(_value);
                            }
                            else
                            {
                                Assert.NotNull(_value);
                            }
                        }
                    }
                }
            }
            Assert.NotNull(types);
            Assert.NotEmpty(types);
        }
    }
}
