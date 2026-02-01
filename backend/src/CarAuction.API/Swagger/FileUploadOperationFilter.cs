using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarAuction.API.Swagger;

/// <summary>
/// Swagger operation filter to properly display file upload parameters
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) ||
                       p.ParameterType == typeof(List<IFormFile>) ||
                       p.ParameterType == typeof(IFormFile[]))
            .ToList();

        if (!fileParams.Any())
            return;

        // Remove any existing parameters for files
        var paramsToRemove = operation.Parameters
            .Where(p => fileParams.Any(fp => fp.Name == p.Name))
            .ToList();

        foreach (var param in paramsToRemove)
        {
            operation.Parameters.Remove(param);
        }

        // Set request body for multipart/form-data
        var properties = new Dictionary<string, OpenApiSchema>();
        var required = new HashSet<string>();

        foreach (var fileParam in fileParams)
        {
            var isMultiple = fileParam.ParameterType != typeof(IFormFile);

            properties[fileParam.Name!] = new OpenApiSchema
            {
                Type = "string",
                Format = "binary",
                Description = isMultiple ? "Multiple image files" : "Image file"
            };

            if (isMultiple)
            {
                properties[fileParam.Name!] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
            }
        }

        // Add non-file query parameters as form fields
        foreach (var param in operation.Parameters.ToList())
        {
            if (param.In == ParameterLocation.Query)
            {
                properties[param.Name] = new OpenApiSchema
                {
                    Type = param.Schema.Type,
                    Default = param.Schema.Default
                };
            }
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = properties,
                        Required = required
                    }
                }
            }
        };
    }
}
