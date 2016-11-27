namespace SoundFingerprinting.Solr.Tests
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using SolrNet;
    using SolrNet.Commands.Parameters;

    using SoundFingerprinting.Math;
    using SoundFingerprinting.Solr.Converters;
    using SoundFingerprinting.Solr.DAO;

    [TestFixture]
    public class SubFingerprintDaoTest
    {
        private Mock<ISolrOperations<SubFingerprintDTO>> solr;
        private Mock<IDictionaryToHashConverter> dictionaryToHashConverter; 
        private Mock<IHashConverter> hashConverter;
        private Mock<ISolrQueryBuilder> solrQueryBuilder;

        private SubFingerprintDao subFingerprintDao;

        [SetUp]
        public void SetUp()
        {
            solr = new Mock<ISolrOperations<SubFingerprintDTO>>(MockBehavior.Strict);
            dictionaryToHashConverter = new Mock<IDictionaryToHashConverter>(MockBehavior.Strict);
            hashConverter = new Mock<IHashConverter>(MockBehavior.Strict);
            solrQueryBuilder = new Mock<ISolrQueryBuilder>(MockBehavior.Strict);

            subFingerprintDao = new SubFingerprintDao(solr.Object, dictionaryToHashConverter.Object, hashConverter.Object, solrQueryBuilder.Object);
        }

        [Test]
        public void ShouldBatchQueriesToSolr()
        {
            List<long[]> hashes = new List<long[]>();
            const int FingerprintsCount = 256;
            for (int i = 0; i < FingerprintsCount; ++i)
            {
                hashes.Add(new long[25]);
            }

            List<int> batchSizes = new List<int>();
            solrQueryBuilder.Setup(bld => bld.BuildReadQueryForHashesAndThreshold(It.IsAny<IEnumerable<long[]>>(), 5))
                                             .Returns("query")
                                             .Callback((IEnumerable<long[]> h, int t) => batchSizes.Add(h.Count()));
            solr.Setup(opr => opr.Query(It.IsAny<SolrQuery>(), It.IsAny<QueryOptions>())).Returns(new SolrQueryResults<SubFingerprintDTO>());

            var allResults = subFingerprintDao.ReadSubFingerprints(hashes, 5, new string[0]);

            CollectionAssert.AreEqual(
                new List<int>
                    {
                        SubFingerprintDao.BatchSize,
                        SubFingerprintDao.BatchSize,
                        FingerprintsCount % SubFingerprintDao.BatchSize
                    },
                batchSizes);
        }

        [Test]
        public void ShouldSpecifyMinimalMatchQueryOption()
        {
            solrQueryBuilder.Setup(bld => bld.BuildReadQueryForHashes(It.IsAny<long[]>())).Returns("query");

            solr.Setup(opr => opr.Query(It.IsAny<SolrQuery>(), It.IsAny<QueryOptions>())).Returns(new SolrQueryResults<SubFingerprintDTO>())
                .Callback((SolrQuery q, QueryOptions opts) =>
                    {
                        Assert.AreEqual("query", q.Query);
                        var options = opts.ExtraParams.ToDictionary(ks => ks.Key, vs => vs.Value);
                        Assert.AreEqual("edismax", options["defType"]);
                        Assert.AreEqual("5", options["mm"]);
                    });
            
            subFingerprintDao.ReadSubFingerprints(new long[25], 5, new string[0]);
        }

        [Test]
        public void ShouldSpecifyFilterQueries()
        {
            solrQueryBuilder.Setup(bld => bld.BuildReadQueryForHashes(It.IsAny<long[]>())).Returns("query");
            solrQueryBuilder.Setup(bld => bld.BuildQueryForClusters(It.IsAny<IEnumerable<string>>())).Returns(
                "filter-query");

            solr.Setup(opr => opr.Query(It.IsAny<SolrQuery>(), It.IsAny<QueryOptions>())).Returns(new SolrQueryResults<SubFingerprintDTO>())
                .Callback((SolrQuery q, QueryOptions opts) =>
                {
                    Assert.AreEqual("query", q.Query);
                    SolrQuery filter = (SolrQuery)opts.FilterQueries.First();
                    Assert.AreEqual("filter-query", filter.Query);
                });

            subFingerprintDao.ReadSubFingerprints(new long[25], 5, new[] { "CA", "LA" });
        }
    }
}
