using Aspire.Hosting.ApplicationModel;
using k8s.KubeConfigModels;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace Aspire.Hosting;

// This class just contains constant strings that can be updated periodically
// when new versions of the underlying container are released.
internal static class LakeFSContainerImageTags
{
    internal const string Registry = "docker.io";

    internal const string Image = "treeverse/lakefs";

    internal const string Tag = "latest";
}

public static class LakeFSResourceBuilderExtensions
{
    private static string CreateSecretKey(int count = 512)
    {
        var bytes = RandomNumberGenerator.GetBytes(count);
        var token = Convert.ToBase64String(bytes);
        return token;
    }

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
    public static IResourceBuilder<LakeFSResource> AddLakeFSQuickstart(
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
            .WithImage(LakeFSContainerImageTags.Image, LakeFSContainerImageTags.Tag)
            .WithImageRegistry(LakeFSContainerImageTags.Registry)
            .WithHttpEndpoint(targetPort: 8000, port: httpPort, name: LakeFSResource.HttpEndpointName)
            .WithArgs(["run", "--quickstart"]);
    }

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
        int? httpPort = null,
        PostgresServerResource? postgres = null
        )
    {
        // The AddResource method is a core API within .NET Aspire and is
        // used by resource developers to wrap a custom resource in an
        // IResourceBuilder<T> instance. Extension methods to customize
        // the resource (if any exist) target the builder interface.
        var resource = new LakeFSResource(name);

        var lakeFS = builder.AddResource(resource)
            .WithImage(LakeFSContainerImageTags.Image, LakeFSContainerImageTags.Tag)
            .WithImageRegistry(LakeFSContainerImageTags.Registry)
            .WithHttpEndpoint(targetPort: 8000, port: httpPort, name: LakeFSResource.HttpEndpointName)
            .WithEnvironment("LAKEFS_AUTH_ENCRYPT_SECRET_KEY", CreateSecretKey())
            .WithEnvironment("LAKEFS_BLOCKSTORE_TYPE", "local")
            .WithArgs(["run"]);

        if (postgres != null)
        {
            lakeFS.WithEnvironment("LAKEFS_DATABASE_TYPE", "postgres");
            builder.Eventing.Subscribe<ResourceReadyEvent>(postgres, async (@event, ct) =>
            {
                var srv = postgres;
                //var connectionString = $"postgres://{database.Name}:{Uri.EscapeDataString(srv.PasswordParameter.Value)}@{srv.Name}:{srv.PrimaryEndpoint.TargetPort}/{database.DatabaseName}";
                var connectionString = $"postgres://{postgres.Name}:{srv.PasswordParameter.Value}@{srv.Name}:{srv.PrimaryEndpoint.TargetPort}/{postgres.Name}";
                lakeFS.WithEnvironment("LAKEFS_DATABASE_POSTGRES_CONNECTION_STRING", connectionString);
            });
        }
        else
        {
            lakeFS.WithEnvironment("LAKEFS_DATABASE_TYPE", "local");
        }

        return lakeFS;
    }
}