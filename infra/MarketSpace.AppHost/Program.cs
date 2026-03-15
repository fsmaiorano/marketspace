using Microsoft.Extensions.Configuration;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
ConfigurationManager config = builder.Configuration;

// Databases - Postgres
IConfigurationSection postgresConfig = config.GetSection("Aspire:Databases:Postgres");
IResourceBuilder<ParameterResource> postgresPassword = builder.AddParameter("postgres-password", "postgres");
IConfigurationSection catalogDbConfig = postgresConfig.GetSection("Catalog");
IResourceBuilder<PostgresDatabaseResource> catalogDb = builder
    .AddPostgres(catalogDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", catalogDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithHostPort(int.Parse(catalogDbConfig["Port"]!))
    .AddDatabase(catalogDbConfig["ConnectionName"]!);

IConfigurationSection orderDbConfig = postgresConfig.GetSection("Order");
IResourceBuilder<PostgresDatabaseResource> orderDb = builder
    .AddPostgres(orderDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", orderDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithHostPort(int.Parse(orderDbConfig["Port"]!))
    .AddDatabase(orderDbConfig["ConnectionName"]!);

IConfigurationSection merchantDbConfig = postgresConfig.GetSection("Merchant");
IResourceBuilder<PostgresDatabaseResource> merchantDb = builder
    .AddPostgres(merchantDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", merchantDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithHostPort(int.Parse(merchantDbConfig["Port"]!))
    .AddDatabase(merchantDbConfig["ConnectionName"]!);

IConfigurationSection userDbConfig = postgresConfig.GetSection("User");
IResourceBuilder<PostgresDatabaseResource> userDb = builder
    .AddPostgres(userDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", userDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithHostPort(int.Parse(userDbConfig["Port"]!))
    .AddDatabase(userDbConfig["ConnectionName"]!);

IConfigurationSection basketDbConfig = postgresConfig.GetSection("Basket");
IResourceBuilder<PostgresDatabaseResource> basketDb = builder
    .AddPostgres(basketDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", basketDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithHostPort(int.Parse(basketDbConfig["Port"]!))
    .AddDatabase(basketDbConfig["ConnectionName"]!);

IConfigurationSection paymentDbConfig = postgresConfig.GetSection("Payment");
IResourceBuilder<PostgresDatabaseResource> paymentDb = builder
    .AddPostgres(paymentDbConfig["Name"]!, password: postgresPassword)
    .WithEnvironment("POSTGRES_DB", paymentDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithHostPort(int.Parse(paymentDbConfig["Port"]!))
    .AddDatabase(paymentDbConfig["ConnectionName"]!);

// Message Broker
IConfigurationSection rabbitMqConfig = config.GetSection("Aspire:MessageBrokers:RabbitMQ");
IResourceBuilder<ContainerResource> rabbitmq = builder.AddContainer(rabbitMqConfig["Name"]!, "rabbitmq", "management")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
    .WithEnvironment("RABBITMQ_DEFAULT_VHOST", "/")
    .WithEndpoint(port: int.Parse(rabbitMqConfig["Port"]!), targetPort: 5672, scheme: "amqp", name: "rabbitmq")
    .WithHttpEndpoint(port: int.Parse(rabbitMqConfig["Ui"]!), targetPort: 15672, name: "rabbitmq-ui");

// AI - Ollama (inference + embeddings) and postgres-ai (vector storage via pgvector)
//
// Consumer NuGet packages for microservices that need AI features:
//   - Microsoft.Extensions.AI.Ollama          → chat completions & embeddings (MEAi abstraction)
//   - Microsoft.SemanticKernel.Connectors.Ollama → Semantic Kernel integration
//   - Pgvector.EntityFrameworkCore            → EF Core vector column support
//   - Npgsql.EntityFrameworkCore.PostgreSQL   → already used; enable UseVector() option
//
// Inject into a service: builder.Services.AddOllamaApiClient(new Uri(builder.Configuration["services__ollama__http__0"]!))
IConfigurationSection aiInfraConfig = config.GetSection("Aspire:AI");
IConfigurationSection ollamaConfig = aiInfraConfig.GetSection("Ollama");
IConfigurationSection aiDbConfig = aiInfraConfig.GetSection("Postgres");

IResourceBuilder<OllamaResource> ollama = builder
    .AddOllama(ollamaConfig["ContainerName"]!, port: int.Parse(ollamaConfig["Port"]!))
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

ollama.AddModel("llama3.2:1b");
ollama.AddModel("nomic-embed-text");

IResourceBuilder<PostgresDatabaseResource> aiDb = builder
    .AddPostgres(aiDbConfig["Name"]!, password: postgresPassword)
    .WithImage("pgvector/pgvector", "pg17")
    .WithEnvironment("POSTGRES_DB", aiDbConfig["DatabaseName"]!)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithHostPort(int.Parse(aiDbConfig["Port"]!))
    .WithBindMount("../../infra/ollama/init-pgvector.sql", "/docker-entrypoint-initdb.d/init-pgvector.sql", isReadOnly: true)
    .AddDatabase(aiDbConfig["ConnectionName"]!);

// Storage - Minio
IConfigurationSection minioConfig = config.GetSection("Aspire:Storage:Minio");
IResourceBuilder<ContainerResource> minio = builder
    .AddContainer(minioConfig["ContainerName"]!, minioConfig["Image"]!, minioConfig["Tag"]!)
    .WithHttpEndpoint(port: int.Parse(minioConfig["ApiPort"]!), targetPort: int.Parse(minioConfig["ApiPort"]!),
        name: "minio-api")
    .WithHttpEndpoint(port: int.Parse(minioConfig["ConsolePort"]!), targetPort: int.Parse(minioConfig["ConsolePort"]!),
        name: "minio-console")
    .WithEnvironment("MINIO_ROOT_USER", minioConfig["RootUser"]!)
    .WithEnvironment("MINIO_ROOT_PASSWORD", minioConfig["RootPassword"]!)
    .WithArgs("server", "/data", "--console-address", $":{minioConfig["ConsolePort"]}")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithVolume("minio-data", "/data");

// Microservices
IConfigurationSection catalogConfig = config.GetSection("Aspire:Services:Catalog");
IResourceBuilder<ProjectResource> catalogApi = builder.AddProject<Projects.Catalog_Api>(catalogConfig["ProjectName"]!)
    .WithReference(catalogDb)
    .WaitFor(rabbitmq)
    .WithEnvironment("ConnectionStrings__RabbitMQ", $"amqp://guest:guest@localhost:{rabbitMqConfig["Port"]!}/")
    .WithEnvironment("Storage__Minio__Endpoint", minioConfig["Endpoint"]!)
    .WithEnvironment("Storage__Minio__AccessKey", minioConfig["RootUser"]!)
    .WithEnvironment("Storage__Minio__SecretKey", minioConfig["RootPassword"]!)
    .WithEnvironment("Storage__Minio__BucketName", minioConfig["BucketName"]!)
    .WithHttpEndpoint(port: int.Parse(catalogConfig["HttpPort"]!), name: "catalog-http")
    .WithHttpsEndpoint(port: int.Parse(catalogConfig["HttpsPort"]!), name: "catalog-https");

IConfigurationSection basketConfig = config.GetSection("Aspire:Services:Basket");
IResourceBuilder<ProjectResource> basketApi = builder.AddProject<Projects.Basket_Api>(basketConfig["ProjectName"]!)
    .WithReference(basketDb)
    .WaitFor(rabbitmq)
    .WithHttpEndpoint(port: int.Parse(basketConfig["HttpPort"]!), name: "basket-http")
    .WithHttpsEndpoint(port: int.Parse(basketConfig["HttpsPort"]!), name: "basket-https");

IConfigurationSection orderConfig = config.GetSection("Aspire:Services:Order");
IResourceBuilder<ProjectResource> orderApi = builder.AddProject<Projects.Order_Api>(orderConfig["ProjectName"]!)
    .WithReference(orderDb)
    .WaitFor(rabbitmq)
    .WithEnvironment("ConnectionStrings__RabbitMQ", $"amqp://guest:guest@localhost:{rabbitMqConfig["Port"]!}/")
    .WithHttpEndpoint(port: int.Parse(orderConfig["HttpPort"]!), name: "order-http")
    .WithHttpsEndpoint(port: int.Parse(orderConfig["HttpsPort"]!), name: "order-https");

IConfigurationSection merchantConfig = config.GetSection("Aspire:Services:Merchant");
IResourceBuilder<ProjectResource> merchantApi = builder
    .AddProject<Projects.Merchant_Api>(merchantConfig["ProjectName"]!)
    .WithReference(merchantDb)
    .WaitFor(rabbitmq)
    .WithHttpEndpoint(port: int.Parse(merchantConfig["HttpPort"]!), name: "merchant-http")
    .WithHttpsEndpoint(port: int.Parse(merchantConfig["HttpsPort"]!), name: "merchant-https");

IConfigurationSection paymentConfig = config.GetSection("Aspire:Services:Payment");
IResourceBuilder<ProjectResource> paymentApi = builder.AddProject<Projects.Payment_Api>(paymentConfig["ProjectName"]!)
    .WithReference(paymentDb)
    .WaitFor(rabbitmq)
    .WithHttpEndpoint(port: int.Parse(paymentConfig["HttpPort"]!), name: "payment-http")
    .WithHttpsEndpoint(port: int.Parse(paymentConfig["HttpsPort"]!), name: "payment-https");

IConfigurationSection userConfig = config.GetSection("Aspire:Services:User");
IResourceBuilder<ProjectResource> userApi = builder.AddProject<Projects.User_Api>(userConfig["ProjectName"]!)
    .WithReference(userDb)
    .WaitFor(rabbitmq)
    .WithHttpEndpoint(port: int.Parse(userConfig["HttpPort"]!), name: "user-http")
    .WithHttpsEndpoint(port: int.Parse(userConfig["HttpsPort"]!), name: "user-https");

IConfigurationSection aiConfig = config.GetSection("Aspire:Services:AI");
IResourceBuilder<ProjectResource> aiApi = builder.AddProject<Projects.Ai_Api>(aiConfig["ProjectName"]!)
    .WithReference(aiDb)
    .WaitFor(ollama)
    .WithEnvironment("OLLAMA_URL", ollamaConfig["Endpoint"]!)
    .WithHttpEndpoint(port: int.Parse(aiConfig["HttpPort"]!), name: "ai-http")
    .WithHttpsEndpoint(port: int.Parse(aiConfig["HttpsPort"]!), name: "ai-https");

// BFF
IConfigurationSection bffConfig = config.GetSection("Aspire:Services:BFF");
IResourceBuilder<ProjectResource> bffApi = builder.AddProject<Projects.BackendForFrontend_Api>(bffConfig["ProjectName"]!)
    .WaitFor(rabbitmq)
    .WithEnvironment("ConnectionStrings__RabbitMQ", $"amqp://guest:guest@localhost:{rabbitMqConfig["Port"]!}/")
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(orderApi)
    .WithReference(merchantApi)
    .WithReference(userApi)
    .WithReference(paymentApi)
    .WithHttpEndpoint(port: int.Parse(bffConfig["HttpPort"]!), name: "bff-http")
    .WithHttpsEndpoint(port: int.Parse(bffConfig["HttpsPort"]!), name: "bff-https");

// Frontend (Vite React app)
IConfigurationSection webAppConfig = config.GetSection("Aspire:Services:WebApp");
builder.AddViteApp(webAppConfig["ProjectName"]!, "../../ui/marketspace-ui", "dev")
    .WithNpm(installArgs: ["--legacy-peer-deps"])
    .WithEndpoint("http", e => e.Port = int.Parse(webAppConfig["HttpPort"]!))
    .WithEnvironment("VITE_BFF_API_URL", $"http://localhost:{bffConfig["HttpPort"]!}")
    .WithEnvironment("VITE_ENV", "aspire")
    .WaitFor(bffApi);

builder.Build().Run();