namespace WeatherDashboard.Infrastructure.IntegrationTests;

[CollectionDefinition("Elasticsearch")]
public sealed class ElasticsearchTestCollectionDefinition : ICollectionFixture<ElasticsearchContainerFixture>;
