using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin(pgAdmin =>
    {
        pgAdmin.WithHostPort(5050);
    });

var db= postgres.AddDatabase("GameStore");

var redis = builder.AddRedis("redis");

builder.AddProject<GameStore_Api>("gamestore-api")
       .WithHttpHealthCheck("/health")
       .WithReference(db)
       .WaitFor(db)
       .WithReference(redis)
       .WaitFor(redis);

builder.Build().Run();
