namespace SoundFingerprinting.Solr.Test
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Practices.ServiceLocation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Ninject;

    using SolrNet;

    using SoundFingerprinting.Solr.DAO;
    using SoundFingerprinting.Solr.Infrastructure;

    [TestClass]
    public class SolrIntTest
    {
        private static readonly SolrModuleLoader ModuleLoader = new SolrModuleLoader();
        private static readonly StandardKernel Kernel = new StandardKernel();

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            ModuleLoader.LoadAssemblyBindings(Kernel);
        }

        [TestMethod]
        public void SolrServerIsAccessible()
        {
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SubFingerprintDTO>>();
            solr.Ping();
        }

        [TestMethod]
        public void SolrServerCanStoreSubFingerprints()
        {
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SubFingerprintDTO>>();

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
            TearDownDocs(solr, docs);
        }

        [TestMethod]
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

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SubFingerprintDTO>>();

            solr.Add(doc1);
            solr.Add(doc2);
            solr.Commit();

            var query = new SolrQuery("{!frange l=4 u=5}sum(termfreq(hashTable_1,'10'),termfreq(hashTable_2,'11'),termfreq(hashTable_3,'12'),termfreq(hashTable_4,'21'), termfreq(hashTable_5,'22'))");
            var docs = solr.Query(query);
           
            Assert.AreEqual(1, docs.Count);
            var result = docs.First();
            Assert.AreEqual(doc2.SubFingerprintId, result.SubFingerprintId);

            TearDownDocs(solr, new List<SubFingerprintDTO> { doc1, doc2 });
        }

        private void TearDownDocs(ISolrOperations<SubFingerprintDTO> solr, IEnumerable<SubFingerprintDTO> subFingerprintDtos)
        {
            solr.Delete(
                subFingerprintDtos.Select(dto => dto.SubFingerprintId));
            solr.Commit();
        }
    }
}
