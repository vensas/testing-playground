namespace Testable.AppHost.Extensions;

public static class ResourceExtensions
{
    public static EndpointReference GetFirstOfEndpoint<T>(this IResourceBuilder<T> builder, string name, params string[] others) 
        where T : IResourceWithEndpoints
    {
        var resource = builder.Resource;
        if (resource.TryGetAnnotationsOfType<EndpointAnnotation>(out var endpoints))
        {
            var endpointsList = endpoints.ToList();
            var result = endpointsList.FirstOrDefault(e => e.Name == name);
            if (result is not null)
                return new EndpointReference(resource, result);

            foreach (var otherName in others)
            {
                result = endpointsList.FirstOrDefault(e => e.Name == otherName);
                if (result is not null)
                    return new EndpointReference(resource, result);
            }
        }

        throw new InvalidOperationException("Endpoint not found.");
    }
}