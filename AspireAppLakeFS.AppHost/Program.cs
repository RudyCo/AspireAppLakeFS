var builder = DistributedApplication.CreateBuilder(args);

// lakeFS Database
var lakefs_postgredb = builder.AddPostgres("lakefs-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgWeb()
    .AddDatabase("lakefs-postgredb", "postgres");

// lakeFS Storage
//var lakefs_storage = builder.AddAzureStorage("lakefs-storage");
//var lakefs_blobstorage = lakefs_storage.AddBlobs("lakefs-blobs");

//if (builder.Environment.IsDevelopment())
//{
//    lakefs_storage.RunAsEmulator(container =>
//    {
//        container.WithDataVolume();
//        container.WithBlobPort(10000);
//        container.WithQueuePort(10001);
//        container.WithTablePort(10002);
//    });
//}

// lakeFS
var lakefs = builder.AddLakeFS("lakefs", database: lakefs_postgredb /*, storage: lakefs_storage*/)
    .WithLifetime(ContainerLifetime.Persistent);

var apiService = builder.AddProject<Projects.AspireAppLakeFS_ApiService>("apiservice")
    .WithEndpoint("https", endpoint => endpoint.IsProxied = false)
    .WithReference(lakefs).WaitFor(lakefs);

builder.Build().Run();