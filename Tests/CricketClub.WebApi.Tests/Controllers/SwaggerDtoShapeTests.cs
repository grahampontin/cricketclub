using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using CricketClub.WebApi.Tests.Utils;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class SwaggerDtoShapeTests
    {
        public SwaggerDtoShapeTests()
        {
            TestDefaults.ResetInternalCache();
        }

        [Fact]
        public void All_Controller_RequestAndResponse_Dtos_ShouldNotExposePublicFields()
        {
            var controllerAssembly = typeof(CricketClub.WebApi.Controllers.StatsController).Assembly;

            var controllerTypes = controllerAssembly
                .GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            var problems = new List<string>();
            var visited = new HashSet<Type>();

            foreach (var controllerType in controllerTypes)
            {
                var actionMethods = controllerType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName)
                    .ToList();

                foreach (var method in actionMethods)
                {
                    // Parameters (request DTOs)
                    foreach (var parameter in method.GetParameters())
                    {
                        VisitType(parameter.ParameterType, $"{controllerType.Name}.{method.Name}({parameter.Name})", problems, visited);
                    }

                    // Return type (response DTOs)
                    VisitType(method.ReturnType, $"{controllerType.Name}.{method.Name} return", problems, visited);
                }
            }

            Assert.True(problems.Count == 0, "Swagger-exposed DTO field issues:" + Environment.NewLine + string.Join(Environment.NewLine, problems));
        }

        private static void VisitType(Type type, string source, List<string> problems, HashSet<Type> visited)
        {
            type = Unwrap(type);

            if (type == null)
            {
                return;
            }

            if (visited.Contains(type))
            {
                return;
            }
            visited.Add(type);

            if (type == typeof(void)) return;
            if (type.IsPrimitive) return;
            if (type.IsEnum) return;
            if (type == typeof(string)) return;
            if (type == typeof(decimal)) return;
            if (type == typeof(DateTime)) return;
            if (type == typeof(DateTimeOffset)) return;
            if (type == typeof(Guid)) return;

            // Only enforce DTO hygiene for API-owned contracts.
            // Some controllers may accept/return internal domain models (e.g. CricketClubDomain.*) that legitimately use fields.
            if (type.Namespace == null || !type.Namespace.StartsWith("CricketClub.WebApi", StringComparison.Ordinal))
            {
                return;
            }

            // Arrays
            if (type.IsArray)
            {
                VisitType(type.GetElementType()!, source, problems, visited);
                return;
            }

            // IEnumerable<T>
            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    VisitType(arg, source, problems, visited);
                }
            }

            // Public instance fields are a red flag: JSON serialization and Swagger may omit them.
            var publicFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in publicFields)
            {
                problems.Add($"{source}: DTO '{type.FullName}' exposes public field '{field.Name}'. Use get/set properties instead.");
            }

            // Recurse into public properties
            var publicProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in publicProps)
            {
                VisitType(prop.PropertyType, $"{source}: {type.Name}.{prop.Name}", problems, visited);
            }
        }

        private static Type? Unwrap(Type type)
        {
            // Task<T>
            if (typeof(System.Threading.Tasks.Task).IsAssignableFrom(type) && type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            // ActionResult<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                return type.GetGenericArguments()[0];
            }

            // Nullable<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        [Fact]
        public void All_Controller_ActionSignatures_ShouldNotExpose_InternalDomainTypes()
        {
            var controllerAssembly = typeof(CricketClub.WebApi.Controllers.StatsController).Assembly;

            var controllerTypes = controllerAssembly
                .GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            var problems = new List<string>();

            foreach (var controllerType in controllerTypes)
            {
                var actionMethods = controllerType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName)
                    .ToList();

                foreach (var method in actionMethods)
                {
                    foreach (var parameter in method.GetParameters())
                    {
                        FindInternalTypes(parameter.ParameterType,
                            $"{controllerType.Name}.{method.Name}({parameter.Name})",
                            problems);
                    }

                    FindInternalTypes(method.ReturnType,
                        $"{controllerType.Name}.{method.Name} return",
                        problems);
                }
            }

            Assert.True(problems.Count == 0, "Internal domain types are exposed in controller action signatures:" + Environment.NewLine + string.Join(Environment.NewLine, problems));
        }

        private static void FindInternalTypes(Type type, string source, List<string> problems)
        {
            var visited = new HashSet<Type>();
            FindInternalTypesRecursive(type, source, problems, visited);
        }

        private static void FindInternalTypesRecursive(Type type, string source, List<string> problems, HashSet<Type> visited)
        {
            type = Unwrap(type) ?? type;

            if (visited.Contains(type)) return;
            visited.Add(type);

            // Unwrap arrays
            if (type.IsArray)
            {
                FindInternalTypesRecursive(type.GetElementType()!, source, problems, visited);
                return;
            }

            // Unwrap generics
            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    FindInternalTypesRecursive(arg, source, problems, visited);
                }
            }

            if (IsInternalDomainNamespace(type.Namespace))
            {
                problems.Add($"{source}: '{type.FullName}' is an internal type. Use a CricketClub.WebApi.* V1 DTO + mapper instead.");
                return;
            }

            // Walk DTO graphs (including WebApi DTOs) to ensure nested properties don't pull in internal types.
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
            {
                FindInternalTypesRecursive(prop.PropertyType, $"{source}: {type.Name}.{prop.Name}", problems, visited);
            }
        }

        private static bool IsInternalDomainNamespace(string? @namespace)
        {
            if (string.IsNullOrWhiteSpace(@namespace)) return false;

            if (@namespace.StartsWith("CricketClub.WebApi", StringComparison.Ordinal)) return false;

            return @namespace.StartsWith("CricketClubDomain", StringComparison.Ordinal)
                   || @namespace.StartsWith("CricketClubMiddle", StringComparison.Ordinal)
                   || @namespace.StartsWith("CricketClubDAL", StringComparison.Ordinal);
        }
    }
}
