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
                    Hashes = new Dictionary<int, long> { { 1, 10 }, { 2, 11 }, { 3, 12 }, { 4, 13 }, { 5, 14 } }
                };

            var doc2 = new SubFingerprintDTO
                {
                    SubFingerprintId = "2",
                    TrackId = "2",
                    Hashes = new Dictionary<int, long> { { 1, 10 }, { 2, 11 }, { 3, 20 }, { 4, 21 }, { 5, 22 } }
                };

            var solr = DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>();

            solr.Add(doc1);
            solr.Add(doc2);
            solr.Commit();

            var query = new SolrQuery("{!frange l=4 u=5}sum(termfreq(hashTable_1,'10'),termfreq(hashTable_2,'11'),termfreq(hashTable_3,'12'),termfreq(hashTable_4,'21'), termfreq(hashTable_5,'22'))");
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
