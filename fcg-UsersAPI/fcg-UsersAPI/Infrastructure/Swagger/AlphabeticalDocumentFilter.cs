using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Infrastructure.Swagger;

public class AlphabeticalDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Ordena as rotas alfabeticamente
        var orderedPaths = swaggerDoc.Paths
            .OrderBy(p => p.Key)
            .ToDictionary(p => p.Key, p => p.Value);

        swaggerDoc.Paths = new OpenApiPaths();

        foreach (var path in orderedPaths)
        {
            // Ordena também os métodos HTTP (GET, POST, PUT, DELETE etc.)
            var orderedOperations = path.Value.Operations
                .OrderBy(o => o.Key.ToString()) // ordena por nome do verbo
                .ToDictionary(o => o.Key, o => o.Value);

            var newPathItem = new OpenApiPathItem();

            foreach (var op in orderedOperations)
                newPathItem.Operations.Add(op.Key, op.Value);

            swaggerDoc.Paths.Add(path.Key, newPathItem);
        }
    }
}
