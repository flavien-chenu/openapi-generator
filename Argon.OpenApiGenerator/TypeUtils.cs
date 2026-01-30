using System.Linq;
using Microsoft.OpenApi;

namespace Argon.OpenApiGenerator;

public static class TypeUtils
{
    public static bool IsNullableType(IOpenApiSchema schema)
    {
        var nullable = schema.Type?.HasFlag(JsonSchemaType.Null) ?? false;

        if (schema.OneOf is { Count: 2 })
        {
            nullable |= schema.OneOf.Any(s => s.Type?.HasFlag(JsonSchemaType.Null) == true);
        }
        return nullable;
    }

    public static bool IsArrayType(IOpenApiSchema schema)
    {
        return schema.Type?.HasFlag(JsonSchemaType.Array) ?? false;
    }

    public static string DetermineFinalType(IOpenApiSchema schema, bool escapeNullable = false)
    {
        while (true)
        {
            if (IsNullableType(schema) && !escapeNullable)
            {
                if (schema.OneOf is null)
                {
                    return DetermineFinalType(schema, true) + "?";
                }
                var other = schema.OneOf.FirstOrDefault(s => !s.Type?.HasFlag(JsonSchemaType.Null) == true);
                if (other != null)
                {
                    return DetermineFinalType(other, true) + "?";
                }
                return "object?";
            }
            var first = schema.AllOf?.FirstOrDefault(s => s.Type != null);
            if (first != null)
            {
                schema = first;
                continue;
            }

            if (schema is OpenApiSchemaReference { Reference.Id: not null } reference)
            {
                return reference.Reference.Id;
            }

            if (IsArrayType(schema))
            {
                var itemType = "object";
                if (schema.Items != null)
                {
                    itemType = DetermineFinalType(schema.Items);
                }
                return $"ICollection<{itemType}>";
            }

            if (schema.Enum != null)
            {
                return "string";
            }

            if (schema.Type?.HasFlag(JsonSchemaType.Integer) == true)
            {
                return schema.Format switch
                {
                    "int64" => "long",
                    _ => "int",
                };
            }

            if (schema.Type?.HasFlag(JsonSchemaType.Number) == true)
            {
                return schema.Format switch
                {
                    "float" => "float",
                    "double" => "double",
                    _ => "decimal",
                };
            }

            if (schema.Type?.HasFlag(JsonSchemaType.String) == true)
            {
                return schema.Format switch
                {
                    "date" => "DateOnly",
                    "date-time" => "DateTime",
                    "uuid" => "Guid",
                    "byte" => "byte[]",
                    _ => "string",
                };
            }

            if (schema.Type?.HasFlag(JsonSchemaType.Boolean) == true)
            {
                return "bool";
            }

            if (schema.Type?.HasFlag(JsonSchemaType.Object) == true && schema.AdditionalProperties != null)
            {
                var valueType = DetermineFinalType(schema.AdditionalProperties);
                return $"IDictionary<string, {valueType}>";
            }
            break;
        }
        return "object";
    }
}
