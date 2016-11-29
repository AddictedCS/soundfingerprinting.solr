namespace SoundFingerprinting.Solr.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using Ninject.Integration.SolrNet.Config;

    using NUnit.Framework;

    using SolrNet;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.DAO;

    [TestFixture]
    [Category("IntegrationTest")]
    public class SolrIntTest
    {
        [Test]
        public void SolrServerIsAccessible()
        {
            var solr = DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>();
            solr.Ping();
        }

        [Test]
        public void SolrServerCanStoreSubFingerprints()
        {
            var solr = DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>();

            solr.Add(
                new SubFingerprintDTO
                    {
                        SubFingerprintId = "321",
                        TrackId = "123",
                        Hashes = new Dictionary<int, long> { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } }
                    });
            solr.Commit();

            var docs = solr.Query("subFingerprintId:321");

            Assert.AreEqual(1, docs.Count);
            this.TearDownDocs(solr, docs);
        }

        [Test]
        public void SolrFrameworkCanRunComplexQueries()
        {
            var doc1 = new SubFingerprintDTO
                {
                    SubFingerprintId = "1",
                    TrackId = "1",
                    Hashes = new Dictionary<int, long> { { 0, 10 }, { 1, 11 }, { 2, 12 }, { 3, 13 }, { 4, 14 } }
                };

            var doc2 = new SubFingerprintDTO
                {
                    SubFingerprintId = "2",
                    TrackId = "2",
                    Hashes = new Dictionary<int, long> { { 0, 10 }, { 1, 11 }, { 2, 20 }, { 3, 21 }, { 4, 22 } }
                };

            var solr = DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>();

            solr.Add(doc1);
            solr.Add(doc2);
            solr.Commit();

            var query = new SolrQuery("_query_:\"{!edismax mm=4}hashTable_0:10 hashTable_1:11 hashTable_2:12 hashTable_3:21 hashTable_4:22\" _query_:\"{!edismax mm=4}hashTable_0:10 hashTable_1:11 hashTable_2:20 hashTable_3:21 hashTable_4:22\"");
            var docs = solr.Query(query);
           
            Assert.AreEqual(1, docs.Count);
            var result = docs.First();
            Assert.AreEqual(doc2.SubFingerprintId, result.SubFingerprintId);

            this.TearDownDocs(solr, new List<SubFingerprintDTO> { doc1, doc2 });
        }

        [Test]
        public void ShouldReadConfigurationEntriesFromConfigFile()
        {
            var config = (SolrConfigurationSection)ConfigurationManager.GetSection("solr");

            Assert.IsNotNull(config);
        }

        [Test]
        public void ShouldLoadConfigEntriesInNinjectKernel()
        {
            var solrForTracks = DependencyResolver.Current.Get<ISolrOperations<TrackDTO>>();
            var solrForSubFingerprints = DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>();

            Assert.IsNotNull(solrForTracks);
            Assert.IsNotNull(solrForSubFingerprints);
        }

        private void TearDownDocs(ISolrOperations<SubFingerprintDTO> solr, IEnumerable<SubFingerprintDTO> subFingerprintDtos)
        {
            solr.Delete(subFingerprintDtos.Select(dto => dto.SubFingerprintId));
            solr.Commit();
        }
    }
}
