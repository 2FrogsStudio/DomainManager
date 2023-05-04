using DomainManager;

var host = Host.CreateApplicationBuilder(args)
    .AddLogging()
    .AddDatabase()
    .AddBusiness()
    .Build();

host
    .MigrateDatabase()
    .Run();