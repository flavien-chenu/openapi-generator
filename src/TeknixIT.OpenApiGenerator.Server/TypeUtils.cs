using System.Linq;
using Microsoft.OpenApi;

namespace TeknixIT.OpenApiGenerator.Server;

/// <summary>
/// Utility class for determining C# types from OpenAPI schema definitions.
/// </summary>
internal static class TypeUtils
{
    /// <summary>
    /// Determines if the schema represents a nullable type.
    /// </summary>
    /// <param name="schema">The OpenAPI schema to check.</param>
    /// <returns>True if the type is nullable, false otherwise.</returns>
    public static bool IsNullableType(IOpenApiSchema schema)
    {
        var nullable = schema.Type?.HasFlag(JsonSchemaType.Null) ?? false;

        if (schema.OneOf is { Count: 2 })
        {
            nullable |= schema.OneOf.Any(s => s.Type?.HasFlag(JsonSchemaType.Null) == true);
        }

        return nullable;
    }

    /// <summary>
    /// Determines if the schema represents an array type.
    /// </summary>
    /// <param name="schema">The OpenAPI schema to check.</param>
    /// <returns>True if the type is an array, false otherwise.</returns>
    public static bool IsArrayType(IOpenApiSchema schema)
    {
        return schema.Type?.HasFlag(JsonSchemaType.Array) ?? false;
    }

    /// <summary>
    /// Determines the final C# type from an OpenAPI schema.
    /// </summary>
    /// <param name="schema">The OpenAPI schema to convert.</param>
    /// <param name="escapeNullable">If true, prevents adding nullable markers.</param>
    /// <returns>The C# type as a string.</returns>
    public static string DetermineFinalType(IOpenApiSchema schema, bool escapeNullable = false)
    {
        while (true)
        {
            // Handle nullable types
            if (IsNullableType(schema) && !escapeNullable)
            {
                return GetNullableType(schema);
            }

            // Handle AllOf - unwrap to the first schema with a type
            var resolvedSchema = ResolveAllOf(schema);
            if (resolvedSchema != schema)
            {
                schema = resolvedSchema;
                continue;
            }

            // Handle schema references
            if (schema is OpenApiSchemaReference { Reference.Id: not null } reference)
            {
                return reference.Reference.Id;
            }

            // Handle array types
            if (IsArrayType(schema))
            {
                return GetArrayType(schema);
            }

            // Handle enum types
            if (schema.Enum != null)
            {
                return "string";
            }

            // Handle primitive types
            return GetPrimitiveType(schema);
        }
    }

    /// <summary>
    /// Resolves the nullable type representation.
    /// </summary>
    private static string GetNullableType(IOpenApiSchema schema)
    {
        if (schema.OneOf is null)
        {
            return DetermineFinalType(schema, escapeNullable: true) + "?";
        }

        var nonNullSchema = schema.OneOf.FirstOrDefault(s => s.Type?.HasFlag(JsonSchemaType.Null) != true);
        if (nonNullSchema != null)
        {
            return DetermineFinalType(nonNullSchema, escapeNullable: true) + "?";
        }

        return "object?";
    }

    /// <summary>
    /// Resolves AllOf schemas to the first schema with a type.
    /// </summary>
    private static IOpenApiSchema ResolveAllOf(IOpenApiSchema schema)
    {
        return schema.AllOf?.FirstOrDefault(s => s.Type != null) ?? schema;
    }

    /// <summary>
    /// Gets the C# array type representation.
    /// </summary>
    private static string GetArrayType(IOpenApiSchema schema)
    {
        var itemType = schema.Items != null
            ? DetermineFinalType(schema.Items)
            : "object";

        return $"ICollection<{itemType}>";
    }

    /// <summary>
    /// Gets the C# primitive type from the schema.
    /// </summary>
    private static string GetPrimitiveType(IOpenApiSchema schema)
    {
        if (schema.Type?.HasFlag(JsonSchemaType.Integer) == true)
        {
            return GetIntegerType(schema.Format);
        }

        if (schema.Type?.HasFlag(JsonSchemaType.Number) == true)
        {
            return GetNumberType(schema.Format);
        }

        if (schema.Type?.HasFlag(JsonSchemaType.String) == true)
        {
            return GetStringType(schema.Format);
        }

        if (schema.Type?.HasFlag(JsonSchemaType.Boolean) == true)
        {
            return "bool";
        }

        if (schema.Type?.HasFlag(JsonSchemaType.Object) == true && schema.AdditionalProperties != null)
        {
            return GetDictionaryType(schema.AdditionalProperties);
        }

        return "object";
    }

    /// <summary>
    /// Gets the C# integer type based on the format.
    /// </summary>
    private static string GetIntegerType(string? format)
    {
        return format switch
        {
            "int64" => "long",
            _ => "int",
        };
    }

    /// <summary>
    /// Gets the C# number type based on the format.
    /// </summary>
    private static string GetNumberType(string? format)
    {
        return format switch
        {
            "float" => "float",
            "double" => "double",
            _ => "decimal",
        };
    }

    /// <summary>
    /// Gets the C# string type based on the format.
    /// </summary>
    private static string GetStringType(string? format)
    {
        return format switch
        {
            "date" => "DateOnly",
            "date-time" => "DateTime",
            "uuid" => "Guid",
            "byte" => "byte[]",
            _ => "string",
        };
    }

    /// <summary>
    /// Gets the C# dictionary type representation.
    /// </summary>
    private static string GetDictionaryType(IOpenApiSchema valueSchema)
    {
        var valueType = DetermineFinalType(valueSchema);
        return $"IDictionary<string, {valueType}>";
    }
}
