using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;


// This class just contains constant strings that can be updated periodically
// when new versions of the underlying container are released.
internal static class LakeFSContainerImageTags
{
    internal const string Registry = "docker.io";

    internal const string Image = "treeverse/lakefs";

    internal const string Tag = "1.39.2";
}

public static class LakeFSResourceBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="LakeFSResource"/> to the given
    /// <paramref name="builder"/> instance. Uses the "2.0.2" tag.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="httpPort">The HTTP port.</param>
    /// <returns>
    /// An <see cref="IResourceBuilder{LakeFSResource}"/> instance that
    /// represents the added lakeFS resource.
    /// </returns>
    public static IResourceBuilder<LakeFSResource> AddLakeFS(
        this IDistributedApplicationBuilder builder,
        string name,
        int? httpPort = null)
    {
        // The AddResource method is a core API within .NET Aspire and is
        // used by resource developers to wrap a custom resource in an
        // IResourceBuilder<T> instance. Extension methods to customize
        // the resource (if any exist) target the builder interface.
        var resource = new LakeFSResource(name);

        return builder.AddResource(resource)
            .WithImage(LakeFSContainerImageTags.Image)
            .WithImageRegistry(LakeFSContainerImageTags.Registry)
            .WithImageTag(LakeFSContainerImageTags.Tag)
            .WithArgs(["run", "--quickstart"])
            .WithHttpEndpoint(
                targetPort: 8000,
                port: httpPort,
                name: LakeFSResource.HttpEndpointName);
    }
}

