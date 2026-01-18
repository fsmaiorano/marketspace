var builder = DistributedApplication.CreateBuilder(args);

var basketDb = builder.AddMongoDB("basketdb")
    .WithDataVolume()
    .AddDatabase("BasketDb");

var catalogDb = builder.AddPostgres("catalogdb")
    .WithDataVolume()
    .AddDatabase("CatalogDb");

var merchantDb = builder.AddPostgres("merchantdb")
    .WithDataVolume()
    .AddDatabase("MerchantDb");

var orderDb = builder.AddPostgres("orderdb")
    .WithDataVolume()
    .AddDatabase("OrderDb");

var userDb = builder.AddPostgres("userdb")
    .WithDataVolume()
    .AddDatabase("UserDb");

var basketApi = builder.AddProject<Projects.Basket_Api>("basket-api")
    .WithReference(basketDb);

var catalogApi = builder.AddProject<Projects.Catalog_Api>("catalog-api")
    .WithReference(catalogDb);

var merchantApi = builder.AddProject<Projects.Merchant_Api>("merchant-api")
    .WithReference(merchantDb);

var orderApi = builder.AddProject<Projects.Order_Api>("order-api")
    .WithReference(orderDb);

var userApi = builder.AddProject<Projects.User>("user-api")
    .WithReference(userDb);

builder.AddProject<Projects.BackendForFrontend_Api>("bff-api")
    .WithReference(basketApi)
    .WithReference(catalogApi)
    .WithReference(merchantApi)
    .WithReference(orderApi)
    .WithReference(userApi);

builder.Build().Run();
