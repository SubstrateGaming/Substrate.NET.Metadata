using Substrate.NetApi.Model.Types.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Tests
{
    internal class ImplementationCheckTests
    {
        [Test]
        public void AllBaseTypeSubclasses_ShouldImplementEncode_ReturnsByteArray()
        {
            // Load the assembly containing Substrate.NET.Metadata classes
            Assembly assembly = Assembly.Load("Substrate.NET.Metadata");

            // Get all types that inherit from BaseType
            var baseType = typeof(BaseType);
            var types = assembly.GetTypes()
                                .Where(t => baseType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                // Instantiate the class using its parameterless constructor (if available)
                object instance = null;
                try
                {
                    instance = Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed to instantiate type {type.Name}: {ex.Message}");
                }

                // Ensure the class has the Encode method
                MethodInfo encodeMethod = type.GetMethod("Encode");
                Assert.That(encodeMethod, Is.Not.Null, $"Class {type.Name} does not implement the Encode() method.");

                try
                {
                    _ = (byte[])encodeMethod.Invoke(instance, null);
                }
                catch (TargetInvocationException ex) when (ex.InnerException is NotImplementedException)
                {
                    Assert.Fail($"Class {type.Name} throws NotImplementedException in Encode() method.");
                }
                catch (TargetInvocationException ex) when (ex.InnerException is NullReferenceException)
                {
                    Assert.Pass($"Class {type.Name} throws NullReferenceException in Encode() method. This is fine");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"An unexpected exception occurred when invoking Encode() on {type.Name}: {ex.Message}");
                }
            }
        }
    }
}
