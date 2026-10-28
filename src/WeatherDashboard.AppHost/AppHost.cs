using Projects;

#pragma warning disable MA0048
IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
#pragma warning restore MA0048

IResourceBuilder<RedisResource> redisCache = builder.AddRedis("redis-cache")
                                                    .WithImageTag("8.8-alpine")
                                                    .WithDataVolume()
                                                    .WithArgs("--maxmemory", "256mb", "--maxmemory-policy", "allkeys-lru")
                                                    .WithRedisInsight(insight =>
                                                         insight.WithImageTag("3.8")
                                                                .WithDataVolume()
                                                                .WithEnvironment("RI_ACCEPT_EULA", "true"));

IResourceBuilder<RedisResource> redisConfig = builder.AddRedis("redis-config")
                                                     .WithImageTag("8.8-alpine")
                                                     .WithDataVolume()
                                                     .WithArgs("--maxmemory", "128mb", "--maxmemory-policy", "noeviction");

IResourceBuilder<ElasticsearchResource> elasticSearch = builder.AddElasticsearch("elasticsearch")
                                                               .WithImageTag("9.5.3")
                                                               .WithDataVolume()
                                                               .WithEnvironment("discovery.type", "single-node")
                                                               .WithEnvironment("xpack.security.enabled", "false");

builder.AddProject<WeatherDashboard_Web>("app")
       .WithReference(redisCache)
       .WithReference(redisConfig)
       .WithReference(elasticSearch)
       .WaitFor(redisCache)
       .WaitFor(redisConfig)
       .WaitFor(elasticSearch);

await builder.Build().RunAsync().ConfigureAwait(false);
