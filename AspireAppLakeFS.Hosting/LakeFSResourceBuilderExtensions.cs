using Aspire.Hosting.ApplicationModel;
using System.Security.Cryptography;

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Aspire.Hosting;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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
        IResourceBuilder<PostgresDatabaseResource>? database = null)
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

        if (database == null)
            lakeFS.WithEnvironment("LAKEFS_DATABASE_TYPE", "local");
        else
            AddLakeFSDatabase(builder, database, lakeFS);

        return lakeFS;
    }

    private static void AddLakeFSDatabase(IDistributedApplicationBuilder builder, IResourceBuilder<PostgresDatabaseResource> database, IResourceBuilder<LakeFSResource> lakeFS)
    {
        lakeFS.WithEnvironment("LAKEFS_DATABASE_TYPE", "postgres");
        lakeFS.WaitFor(database);

        var db = database.Resource;
        builder.Eventing.Subscribe<ResourceReadyEvent>(db, async (@event, ct) =>
            await Task.Run(() =>
            {
                if (!ct.IsCancellationRequested)
                {
                    var srv = db.Parent;
                    var connectionString = $"postgres://{db.DatabaseName}:{srv.PasswordParameter.Value}@{srv.Name}:{srv.PrimaryEndpoint.TargetPort}/{db.DatabaseName}";
                    lakeFS.WithEnvironment("LAKEFS_DATABASE_POSTGRES_CONNECTION_STRING", connectionString);
                }
                return Task.CompletedTask;
            })
        );
    }
}