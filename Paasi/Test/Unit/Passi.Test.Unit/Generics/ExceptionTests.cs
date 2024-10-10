using System.Reflection;
using System.Runtime.Serialization;

namespace Passi.Test.Unit.Generics
{
    public class ExceptionTests
    {
        [Theory]
        [InlineData("Passi.Core")]
        public void Exceptions_Construct_OK(string assemblyName)
        {
            Assembly assembly = Assembly.Load(assemblyName);
            List<Type> types = assembly
                .GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Exception)))
                .ToList();
            foreach (Type type in types)
            {
                var si = new SerializationInfo(type, new FormatterConverter());
                var sc = new StreamingContext();
                try
                {
                    Exception? c1 = Activator.CreateInstance(type) as Exception;
                    Assert.NotNull(c1);
                    Assert.True(!string.IsNullOrWhiteSpace(c1.Message));
                }
                catch (MissingMethodException)
                {
                    // Do nothing
                }

                try
                {
                    var c2 = Activator.CreateInstance(type, "test") as Exception;
                    c2?.GetObjectData(si, sc);
                    Assert.NotNull(c2);
                    Assert.True(!string.IsNullOrWhiteSpace(c2.Message));
                }
                catch (MissingMethodException)
                {
                    // Do nothing
                }

                try
                {
                    var c3 = Activator.CreateInstance(type, "test", 1) as Exception;
                    Assert.True(!string.IsNullOrWhiteSpace(c3?.Message));
                }
                catch (MissingMethodException)
                {
                    // Do nothing
                }
            }
            Assert.NotNull(types);
            Assert.NotEmpty(types);
        }
    }
}
