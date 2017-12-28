namespace SoundFingerprinting.Solr.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Converters;
    using DAO;
    using Math;
    using Moq;
    using NUnit.Framework;

    using SolrNet;
    using SolrNet.Commands.Parameters;

    using SoundFingerprinting.Solr.Config;

    [TestFixture]
    public class SubFingerprintDaoTest
    {
        private Mock<ISolrOperations<SubFingerprintDTO>> solr;
        private Mock<IDictionaryToHashConverter> dictionaryToHashConverter; 
        private Mock<IHashConverter> hashConverter;
        private Mock<ISolrQueryBuilder> solrQueryBuilder;
        private SoundFingerprintingSolrConfiguration solrConfig;

        private SubFingerprintDao subFingerprintDao;

        [SetUp]
        public void SetUp()
        {
            solr = new Mock<ISolrOperations<SubFingerprintDTO>>(MockBehavior.Strict);
            dictionaryToHashConverter = new Mock<IDictionaryToHashConverter>(MockBehavior.Strict);
            hashConverter = new Mock<IHashConverter>(MockBehavior.Strict);
            solrQueryBuilder = new Mock<ISolrQueryBuilder>(MockBehavior.Strict);
            solrConfig = new SoundFingerprintingSolrConfiguration();

            subFingerprintDao = new SubFingerprintDao(this.solr.Object, this.dictionaryToHashConverter.Object, this.solrQueryBuilder.Object, this.solrConfig);
        }

        [Test]
        public void ShouldBatchQueriesToSolr()
        {
            var hashes = new List<int[]>();
            const int FingerprintsCount = 256;
            for (int i = 0; i < FingerprintsCount; ++i)
            {
                hashes.Add(new int[25]);
            }

            List<int> batchSizes = new List<int>();
            solrQueryBuilder.Setup(bld => bld.BuildReadQueryForHashesAndThreshold(It.IsAny<IEnumerable<int[]>>(), 5))
                                             .Returns("query")
                                             .Callback((IEnumerable<int[]> h, int t) => batchSizes.Add(h.Count()));
            solrQueryBuilder.Setup(bld => bld.BuildQueryForClusters(new[] { "CA" })).Returns("clusters:(CA)");
            solr.Setup(opr => opr.Query(It.IsAny<SolrQuery>(), It.IsAny<QueryOptions>())).Returns(new SolrQueryResults<SubFingerprintDTO>())
                .Callback((SolrQuery q, QueryOptions opts) =>
                    {
                        Assert.AreEqual("query", q.Query);
                        var options = opts.ExtraParams.ToDictionary(ks => ks.Key, vs => vs.Value);
                        Assert.AreEqual("true", options["preferLocalShards"]);
                        var filters = opts.FilterQueries;
                        Assert.AreEqual(1, filters.Count);
                        Assert.AreEqual("clusters:(CA)", ((SolrQuery)filters.First()).Query);
                    });

            const int BatchSize = 50;
            solrConfig.QueryBatchSize = BatchSize;
            solrConfig.PreferLocalShards= true;

            subFingerprintDao.ReadSubFingerprints(hashes, 5, new[] { "CA" });

            CollectionAssert.AreEqual(new List<int> { BatchSize, BatchSize, BatchSize, BatchSize, BatchSize, FingerprintsCount % BatchSize }, batchSizes);
        }

        [Test]
        public void ShouldSpecifyMinimalMatchQueryOption()
        {
            solrQueryBuilder.Setup(bld => bld.BuildReadQueryForHashes(It.IsAny<int[]>())).Returns("query");

            solr.Setup(opr => opr.Query(It.IsAny<SolrQuery>(), It.IsAny<QueryOptions>())).Returns(new SolrQueryResults<SubFingerprintDTO>())
                .Callback((SolrQuery q, QueryOptions opts) =>
                    {
                        Assert.AreEqual("query", q.Query);
                        var options = opts.ExtraParams.ToDictionary(ks => ks.Key, vs => vs.Value);
                        Assert.AreEqual("edismax", options["defType"]);
                        Assert.AreEqual("5", options["mm"]);
                    });
            
            subFingerprintDao.ReadSubFingerprints(new int[25], 5, new string[0]);
        }

        [Test]
        public void ShouldSpecifyFilterQueries()
        {
            solrQueryBuilder.Setup(bld => bld.BuildReadQueryForHashes(It.IsAny<int[]>())).Returns("query");
            solrQueryBuilder.Setup(bld => bld.BuildQueryForClusters(It.IsAny<IEnumerable<string>>())).Returns(
                "filter-query");

            solr.Setup(opr => opr.Query(It.IsAny<SolrQuery>(), It.IsAny<QueryOptions>())).Returns(new SolrQueryResults<SubFingerprintDTO>())
                .Callback((SolrQuery q, QueryOptions opts) =>
                {
                    Assert.AreEqual("query", q.Query);
                    SolrQuery filter = (SolrQuery)opts.FilterQueries.First();
                    Assert.AreEqual("filter-query", filter.Query);
                });

            subFingerprintDao.ReadSubFingerprints(new int[25], 5, new[] { "CA", "LA" });
        }

        [Test]
        public void ShouldReadSubFingerprintsByReference()
        {
            var trackReference = new SolrModelReference("track-id");
            var results = new SolrQueryResults<SubFingerprintDTO>();
            var dto = new SubFingerprintDTO
                          {
                              Clusters = new[] { "CA" },
                              SubFingerprintId = "123-123",
                              Hashes = new Dictionary<int, int>(),
                              SequenceAt = 10d,
                              SequenceNumber = 10,
                              TrackId = "track-id"
                          };
            results.AddRange(new List<SubFingerprintDTO> { dto });
            solr.Setup(s => s.Query(It.IsAny<SolrQuery>(), It.IsAny<QueryOptions>())).Returns(results);
            dictionaryToHashConverter.Setup(dhc => dhc.FromSolrDictionaryToHashes(It.IsAny<IDictionary<int, int>>()))
                .Returns(new int[0]);
            hashConverter.Setup(hc => hc.ToBytes(It.IsAny<long[]>(), It.IsAny<int>())).Returns(new byte[0]);

            var subs = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference);

            Assert.AreEqual(1, subs.Count);
            CollectionAssert.AreEqual(new[] { "CA" }, subs.First().Clusters);
        }
    }
}
