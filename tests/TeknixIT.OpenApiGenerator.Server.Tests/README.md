# OpenAPI Server Generator Tests

This directory contains the test suite for the TeknixIT OpenAPI Generator.

## Test Structure

The tests are organized into specialized test classes, each focusing on a specific aspect of the generator:

### Test Classes

- **`TestBase.cs`**: Base class providing common utilities, helper methods, and test infrastructure
  - Generator execution helpers
  - Assertion utilities
  - Test data management
  - Mock classes for Roslyn analyzer testing

- **`ContractGenerationTests.cs`**: Tests for schema/contract generation
  - Simple and complex schema generation
  - Enum generation
  - Nested objects
  - Data type mappings (string, number, date-time, etc.)
  - Required vs optional properties

- **`ControllerGenerationTests.cs`**: Tests for controller generation
  - Controller class generation
  - HTTP method attributes (GET, POST, PUT, DELETE)
  - Parameter types and bindings (FromRoute, FromQuery, FromBody)
  - Async vs sync methods
  - Base class inheritance

- **`ConfigurationTests.cs`**: Tests for generator configuration options
  - Custom namespaces
  - Controller generation on/off
  - Async/sync controller methods
  - ApiController attribute
  - Custom base classes

- **`ValidationTests.cs`**: Tests for validation attribute generation
  - StringLength attributes
  - Range attributes
  - Required attributes
  - RegularExpression attributes for pattern validation
  - Validation enabled/disabled scenarios

- **`TypeMappingTests.cs`**: Tests for OpenAPI type mappings to C# types
  - Primitive type mappings (string, integer, number, boolean)
  - Format-specific mappings (uuid → Guid, date → DateOnly, date-time → DateTime, byte → byte[])
  - Number format mappings (float, double, decimal)
  - Integer format mappings (int32 → int, int64 → long)
  - Dictionary mappings (additionalProperties)
  - Record vs class generation

- **`AdvancedFeaturesTests.cs`**: Tests for advanced OpenAPI features
  - Nullable types with oneOf
  - Nullable properties using nullable: true
  - Required vs optional properties
  - Required modifier generation

- **`ErrorHandlingTests.cs`**: Tests for error scenarios
  - Invalid YAML files
  - Non-existent files
  - Empty API specifications

- **`JsonFormatTests.cs`**: Tests for OpenAPI specifications in JSON format
  - JSON file parsing
  - Contract generation from JSON
  - Controller generation from JSON
  - Enum generation from JSON

- **`ControllerGroupingTests.cs`**: Tests for controller grouping strategies
  - ByTag grouping
  - ByPath grouping
  - ByFirstPathSegment grouping
  - Default grouping behavior

## Test Data

Test data files are located in the `TestData/` directory:

**YAML files:**
- `simple-api.yaml`: Basic API with a single User resource
- `complex-api.yaml`: Complex API with Products, multiple HTTP methods, and various parameter types
- `enum-api.yaml`: API demonstrating enum generation
- `validation-api.yaml`: API with validation constraints (StringLength, Range, RegularExpression)
- `grouping-api.yaml`: API for testing controller grouping strategies
- `nested-api.yaml`: API with nested object schemas
- `empty-api.yaml`: Empty API specification
- `types-api.yaml`: API demonstrating all supported type mappings
- `advanced-features-api.yaml`: API with advanced features (nullable types, oneOf)

**JSON files:**
- `simple-api.json`: Basic API in JSON format
- `enum-api.json`: Enum API in JSON format
- `nested-api.json`: Nested objects API in JSON format

## Running Tests

```bash
# Run all tests
dotnet test

# Run a specific test class
dotnet test --filter FullyQualifiedName~ContractGenerationTests

# Run a specific test
dotnet test --filter FullyQualifiedName~SimpleApi_ShouldGenerateUserContract
```

## Test Conventions

1. **Naming**: Tests follow the pattern `[Scenario]_[Condition]_[ExpectedResult]`
2. **Arrangement**: Each test clearly separates Arrange, Act, and Assert sections
3. **Assertions**: Use descriptive assertion messages
4. **Test Data**: Reuse common test data files when possible
5. **Cleanup**: Tests handle their own cleanup (e.g., temporary files)

## Adding New Tests

When adding new tests:

1. Choose the appropriate test class based on what you're testing
2. If testing a new category, consider creating a new test class
3. Inherit from `TestBase` to access common utilities
4. Follow existing naming and structure conventions
5. Add any new test data files to the `TestData/` directory
6. Update this README if adding a new test class

## Helper Methods

The `TestBase` class provides several helper methods:

- `CreateDefaultConfiguration()`: Creates standard generator configuration
- `RunGenerator()`: Executes the source generator
- `AssertNoErrors()`: Verifies no errors were generated
- `GetGeneratedSource()`: Retrieves a generated file by name
- `AssertSourceEquals()`: Compares generated source with expected content
- `AssertContainsLine()`: Checks for a specific line in generated code
- `AssertContainsLinesInOrder()`: Verifies lines appear in order
- `AssertLineAfter()`: Ensures one line appears after another
