var builder = DistributedApplication.CreateBuilder(args);
var config = builder.Configuration;

// Databases - Postgres
var postgresConfig = config.GetSection("Aspire:Databases:Postgres");
var postgresPassword = builder.AddParameter("postgres-password", "postgres");

var catalogDbConfig = postgresConfig.GetSection("Catalog");
var catalogDb = builder.AddPostgres(catalogDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", catalogDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(int.Parse(catalogDbConfig["Port"]!))
    .AddDatabase(catalogDbConfig["ConnectionName"]!);

var orderDbConfig = postgresConfig.GetSection("Order");
var orderDb = builder.AddPostgres(orderDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", orderDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(int.Parse(orderDbConfig["Port"]!))
    .AddDatabase(orderDbConfig["ConnectionName"]!);

var merchantDbConfig = postgresConfig.GetSection("Merchant");
var merchantDb = builder.AddPostgres(merchantDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", merchantDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(int.Parse(merchantDbConfig["Port"]!))
    .AddDatabase(merchantDbConfig["ConnectionName"]!);

var userDbConfig = postgresConfig.GetSection("User");
var userDb = builder.AddPostgres(userDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", userDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(int.Parse(userDbConfig["Port"]!))
    .AddDatabase(userDbConfig["ConnectionName"]!);

var mongoDbConfig = config.GetSection("Aspire:Databases:MongoDB");
var mongoServer = builder.AddMongoDB(mongoDbConfig["Name"]!, int.Parse(mongoDbConfig["Port"]!))
    .WithLifetime(ContainerLifetime.Persistent);
var basketDb = mongoServer.AddDatabase(mongoDbConfig.GetSection("Basket")["DatabaseName"]!);

// Storage - Minio
var minioConfig = config.GetSection("Aspire:Storage:Minio");
var minio = builder.AddContainer(minioConfig["ContainerName"]!, minioConfig["Image"]!, minioConfig["Tag"]!)
    .WithHttpEndpoint(port: int.Parse(minioConfig["ApiPort"]!), targetPort: int.Parse(minioConfig["ApiPort"]!), name: "minio-api")
    .WithHttpEndpoint(port: int.Parse(minioConfig["ConsolePort"]!), targetPort: int.Parse(minioConfig["ConsolePort"]!), name: "minio-console")
    .WithEnvironment("MINIO_ROOT_USER", minioConfig["RootUser"]!)
    .WithEnvironment("MINIO_ROOT_PASSWORD", minioConfig["RootPassword"]!)
    .WithArgs("server", "/data", "--console-address", $":{minioConfig["ConsolePort"]}")
    .WithLifetime(ContainerLifetime.Persistent);

// Microservices
var catalogConfig = config.GetSection("Aspire:Services:Catalog");
var catalogApi = builder.AddProject<Projects.Catalog_Api>(catalogConfig["ProjectName"]!)
    .WithReference(catalogDb)
    .WithEnvironment("Storage__Minio__Endpoint", minioConfig["Endpoint"]!)
    .WithEnvironment("Storage__Minio__AccessKey", minioConfig["RootUser"]!)
    .WithEnvironment("Storage__Minio__SecretKey", minioConfig["RootPassword"]!)
    .WithEnvironment("Storage__Minio__BucketName", minioConfig["BucketName"]!)
    .WithHttpEndpoint(port: int.Parse(catalogConfig["HttpPort"]!), name: "catalog-http")
    .WithHttpsEndpoint(port: int.Parse(catalogConfig["HttpsPort"]!), name: "catalog-https");

var basketConfig = config.GetSection("Aspire:Services:Basket");
var basketApi = builder.AddProject<Projects.Basket_Api>(basketConfig["ProjectName"]!)
    .WithReference(basketDb)
    .WithEnvironment("DatabaseSettings__DatabaseName", mongoDbConfig.GetSection("Basket")["DatabaseName"]!)
    .WithEnvironment("DatabaseSettings__CollectionName", mongoDbConfig.GetSection("Basket")["CollectionName"]!)
    .WithHttpEndpoint(port: int.Parse(basketConfig["HttpPort"]!), name: "basket-http")
    .WithHttpsEndpoint(port: int.Parse(basketConfig["HttpsPort"]!), name: "basket-https");

var orderConfig = config.GetSection("Aspire:Services:Order");
var orderApi = builder.AddProject<Projects.Order_Api>(orderConfig["ProjectName"]!)
    .WithReference(orderDb)
    .WithHttpEndpoint(port: int.Parse(orderConfig["HttpPort"]!), name: "order-http")
    .WithHttpsEndpoint(port: int.Parse(orderConfig["HttpsPort"]!), name: "order-https");

var merchantConfig = config.GetSection("Aspire:Services:Merchant");
var merchantApi = builder.AddProject<Projects.Merchant_Api>(merchantConfig["ProjectName"]!)
    .WithReference(merchantDb)
    .WithHttpEndpoint(port: int.Parse(merchantConfig["HttpPort"]!), name: "merchant-http")
    .WithHttpsEndpoint(port: int.Parse(merchantConfig["HttpsPort"]!), name: "merchant-https");

var userConfig = config.GetSection("Aspire:Services:User");
var userApi = builder.AddProject<Projects.User>(userConfig["ProjectName"]!)
    .WithReference(userDb)
    .WithHttpEndpoint(port: int.Parse(userConfig["HttpPort"]!), name: "user-http")
    .WithHttpsEndpoint(port: int.Parse(userConfig["HttpsPort"]!), name: "user-https");

// BFF
var bffConfig = config.GetSection("Aspire:Services:BFF");
var bffApi = builder.AddProject<Projects.BackendForFrontend_Api>(bffConfig["ProjectName"]!)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(orderApi)
    .WithReference(merchantApi)
    .WithReference(userApi)
    .WithHttpEndpoint(port: int.Parse(bffConfig["HttpPort"]!), name: "bff-http")
    .WithHttpsEndpoint(port: int.Parse(bffConfig["HttpsPort"]!), name: "bff-https");

builder.Build().Run();
