var host = Host.CreateApplicationBuilder(args)
    .AddLogging()
    .AddMemoryCache()
    .AddDatabase()
    .AddBusiness()
    .Build();

host
    .MigrateDatabase()
    .Run();