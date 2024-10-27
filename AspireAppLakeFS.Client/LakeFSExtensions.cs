using AspireAppLakeFS.Client;
using AspireAppLakeFS.Client.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.Hosting;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering a <see cref="HttpClient"/> as a
/// scoped-lifetime service in the services provided by the <see cref="IHostApplicationBuilder"/>.
/// </summary>
public static class LakeFSExtensions
{
    /// <summary>
    /// Registers 'Scoped' <see cref="LakeFSClientFactory" /> for creating
    /// connected <see cref="HttpClient"/> instance.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IHostApplicationBuilder" /> to read config from and add services to.
    /// </param>
    /// <param name="connectionName">
    /// A name used to retrieve the connection string from the ConnectionStrings configuration section.
    /// </param>
    /// <param name="configureSettings">
    /// An optional delegate that can be used for customizing options.
    /// It's invoked after the settings are read from the configuration.
    /// </param>
    public static void AddLakeFSClient(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<LakeFSClientSettings>? configureSettings = null) =>
        AddLakeFSClient(
            builder,
            LakeFSClientSettings.DefaultConfigSectionName,
            configureSettings,
            connectionName,
            serviceKey: null);

    /// <summary>
    /// Registers 'Scoped' <see cref="LakeFSClientFactory" /> for creating
    /// connected <see cref="HttpClient"/> instance.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IHostApplicationBuilder" /> to read config from and add services to.
    /// </param>
    /// <param name="name">
    /// The name of the component, which is used as the <see cref="ServiceDescriptor.ServiceKey"/> of the
    /// service and also to retrieve the connection string from the ConnectionStrings configuration section.
    /// </param>
    /// <param name="configureSettings">
    /// An optional method that can be used for customizing options. It's invoked after the settings are
    /// read from the configuration.
    /// </param>
    public static void AddKeyedLakeFSClient(
        this IHostApplicationBuilder builder,
        string name,
        Action<LakeFSClientSettings>? configureSettings = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        AddLakeFSClient(
            builder,
            $"{LakeFSClientSettings.DefaultConfigSectionName}:{name}",
            configureSettings,
            connectionName: name,
            serviceKey: name);
    }

    private static void AddLakeFSClient(
        this IHostApplicationBuilder builder,
        string configurationSectionName,
        Action<LakeFSClientSettings>? configureSettings,
        string connectionName,
        object? serviceKey)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var settings = new LakeFSClientSettings();

        builder.Configuration
               .GetSection(configurationSectionName)
               .Bind(settings);

        if (builder.Configuration.GetConnectionString(connectionName) is string connectionString)
        {
            settings.ParseConnectionString(connectionString);
        }

        configureSettings?.Invoke(settings);

        if (serviceKey is null)
        {
            builder.Services.AddScoped(CreateLakeFSClientFactory);
        }
        else
        {
            builder.Services.AddKeyedScoped(serviceKey, (sp, key) => CreateLakeFSClientFactory(sp));
        }

        LakeFSClientFactory CreateLakeFSClientFactory(IServiceProvider _)
        {
            return new LakeFSClientFactory(settings);
        }

        if (settings.DisableHealthChecks is false)
        {
            builder.Services.AddHealthChecks()
                .AddCheck<LakeFSHealthCheck>(
                    name: serviceKey is null ? "LakeFS" : $"LakeFS_{connectionName}",
                    failureStatus: default,
                    tags: []);
        }
    }
}