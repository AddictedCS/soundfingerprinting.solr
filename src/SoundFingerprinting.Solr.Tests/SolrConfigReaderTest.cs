namespace SoundFingerprinting.Solr.Tests
{
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Solr.Config;

    [TestFixture]
    public class SolrConfigReaderTest
    {
        [Test]
        public void ShouldReadFromConfig()
        {
            var solrConfig = SolrConfigReader.GetSolrConfig();

            Assert.IsNotNull(solrConfig);
        }

        [Test]
        public void ShouldReadSolrServersFromConfig()
        {
            var solrServers = SolrConfigReader.GetServersFromLocalConfig().ToList();
            
            Assert.AreEqual(2, solrServers.Count);
        }
    }
}