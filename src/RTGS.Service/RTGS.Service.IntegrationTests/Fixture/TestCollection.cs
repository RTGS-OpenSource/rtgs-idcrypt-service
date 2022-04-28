using Xunit;

namespace RTGS.Service.IntegrationTests.Fixture;

[CollectionDefinition("TestCollection")]
public class TestCollection : ICollectionFixture<TestFixture>
{
}
