using Microsoft.Extensions.Configuration;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
ConfigurationManager config = builder.Configuration;

// Databases - Postgres
IConfigurationSection postgresConfig = config.GetSection("Aspire:Databases:Postgres");
IResourceBuilder<ParameterResource> postgresPassword = builder.AddParameter("postgres-password", "postgres");

IConfigurationSection catalogDbConfig = postgresConfig.GetSection("Catalog");
IResourceBuilder<PostgresDatabaseResource> catalogDb = builder.AddPostgres(catalogDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", catalogDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(int.Parse(catalogDbConfig["Port"]!))
    .AddDatabase(catalogDbConfig["ConnectionName"]!);

IConfigurationSection orderDbConfig = postgresConfig.GetSection("Order");
IResourceBuilder<PostgresDatabaseResource> orderDb = builder.AddPostgres(orderDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", orderDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(int.Parse(orderDbConfig["Port"]!))
    .AddDatabase(orderDbConfig["ConnectionName"]!);

IConfigurationSection merchantDbConfig = postgresConfig.GetSection("Merchant");
IResourceBuilder<PostgresDatabaseResource> merchantDb = builder.AddPostgres(merchantDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", merchantDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(int.Parse(merchantDbConfig["Port"]!))
    .AddDatabase(merchantDbConfig["ConnectionName"]!);

IConfigurationSection userDbConfig = postgresConfig.GetSection("User.Api");
IResourceBuilder<PostgresDatabaseResource> userDb = builder.AddPostgres(userDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", userDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(int.Parse(userDbConfig["Port"]!))
    .AddDatabase(userDbConfig["ConnectionName"]!);

IConfigurationSection basketDbConfig = postgresConfig.GetSection("Basket");
IResourceBuilder<PostgresDatabaseResource> basketDb = builder.AddPostgres(basketDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", basketDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(int.Parse(basketDbConfig["Port"]!))
    .AddDatabase(basketDbConfig["ConnectionName"]!);

// Storage - Minio
IConfigurationSection minioConfig = config.GetSection("Aspire:Storage:Minio");
IResourceBuilder<ContainerResource> minio = builder.AddContainer(minioConfig["ContainerName"]!, minioConfig["Image"]!, minioConfig["Tag"]!)
    .WithHttpEndpoint(port: int.Parse(minioConfig["ApiPort"]!), targetPort: int.Parse(minioConfig["ApiPort"]!), name: "minio-api")
    .WithHttpEndpoint(port: int.Parse(minioConfig["ConsolePort"]!), targetPort: int.Parse(minioConfig["ConsolePort"]!), name: "minio-console")
    .WithEnvironment("MINIO_ROOT_USER", minioConfig["RootUser"]!)
    .WithEnvironment("MINIO_ROOT_PASSWORD", minioConfig["RootPassword"]!)
    .WithArgs("server", "/data", "--console-address", $":{minioConfig["ConsolePort"]}")
    .WithLifetime(ContainerLifetime.Persistent);

// Microservices
IConfigurationSection catalogConfig = config.GetSection("Aspire:Services:Catalog");
IResourceBuilder<ProjectResource> catalogApi = builder.AddProject<Projects.Catalog_Api>(catalogConfig["ProjectName"]!)
    .WithReference(catalogDb)
    .WithEnvironment("Storage__Minio__Endpoint", minioConfig["Endpoint"]!)
    .WithEnvironment("Storage__Minio__AccessKey", minioConfig["RootUser"]!)
    .WithEnvironment("Storage__Minio__SecretKey", minioConfig["RootPassword"]!)
    .WithEnvironment("Storage__Minio__BucketName", minioConfig["BucketName"]!)
    .WithHttpEndpoint(port: int.Parse(catalogConfig["HttpPort"]!), name: "catalog-http")
    .WithHttpsEndpoint(port: int.Parse(catalogConfig["HttpsPort"]!), name: "catalog-https");

IConfigurationSection basketConfig = config.GetSection("Aspire:Services:Basket");
IResourceBuilder<ProjectResource> basketApi = builder.AddProject<Projects.Basket_Api>(basketConfig["ProjectName"]!)
    .WithReference(basketDb)
    .WithHttpEndpoint(port: int.Parse(basketConfig["HttpPort"]!), name: "basket-http")
    .WithHttpsEndpoint(port: int.Parse(basketConfig["HttpsPort"]!), name: "basket-https");

IConfigurationSection orderConfig = config.GetSection("Aspire:Services:Order");
IResourceBuilder<ProjectResource> orderApi = builder.AddProject<Projects.Order_Api>(orderConfig["ProjectName"]!)
    .WithReference(orderDb)
    .WithHttpEndpoint(port: int.Parse(orderConfig["HttpPort"]!), name: "order-http")
    .WithHttpsEndpoint(port: int.Parse(orderConfig["HttpsPort"]!), name: "order-https");

IConfigurationSection merchantConfig = config.GetSection("Aspire:Services:Merchant");
IResourceBuilder<ProjectResource> merchantApi = builder.AddProject<Projects.Merchant_Api>(merchantConfig["ProjectName"]!)
    .WithReference(merchantDb)
    .WithHttpEndpoint(port: int.Parse(merchantConfig["HttpPort"]!), name: "merchant-http")
    .WithHttpsEndpoint(port: int.Parse(merchantConfig["HttpsPort"]!), name: "merchant-https");

IConfigurationSection userConfig = config.GetSection("Aspire:Services:User.Api");
IResourceBuilder<ProjectResource> userApi = builder.AddProject<Projects.User_Api>(userConfig["ProjectName"]!)
    .WithReference(userDb)
    .WithHttpEndpoint(port: int.Parse(userConfig["HttpPort"]!), name: "user-http")
    .WithHttpsEndpoint(port: int.Parse(userConfig["HttpsPort"]!), name: "user-https");

// BFF
IConfigurationSection bffConfig = config.GetSection("Aspire:Services:BFF");
IResourceBuilder<ProjectResource> _ = builder.AddProject<Projects.BackendForFrontend_Api>(bffConfig["ProjectName"]!)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(orderApi)
    .WithReference(merchantApi)
    .WithReference(userApi)
    .WithHttpEndpoint(port: int.Parse(bffConfig["HttpPort"]!), name: "bff-http")
    .WithHttpsEndpoint(port: int.Parse(bffConfig["HttpsPort"]!), name: "bff-https");

builder.Build().Run();
