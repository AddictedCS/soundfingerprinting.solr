namespace SoundFingerprinting.Solr.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Converters;
    using DAO;

    using Moq;
    using NUnit.Framework;

    using SolrNet;
    using SolrNet.Commands.Parameters;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Solr.Config;

    [TestFixture]
    public class SubFingerprintDaoTest
    {
        private Mock<ISolrOperations<SubFingerprintDTO>> solr;
        private Mock<IDictionaryToHashConverter> dictionaryToHashConverter; 
        private Mock<ISolrQueryBuilder> solrQueryBuilder;
        private Mock<ISoundFingerprintingSolrConfig> solrConfig;

        private SubFingerprintDao subFingerprintDao;

        [SetUp]
        public void SetUp()
        {
            solr = new Mock<ISolrOperations<SubFingerprintDTO>>(MockBehavior.Strict);
            dictionaryToHashConverter = new Mock<IDictionaryToHashConverter>(MockBehavior.Strict);
            solrQueryBuilder = new Mock<ISolrQueryBuilder>(MockBehavior.Strict);
            solrConfig = new Mock<ISoundFingerprintingSolrConfig>(MockBehavior.Strict);

            subFingerprintDao = new SubFingerprintDao(this.solr.Object, this.dictionaryToHashConverter.Object, this.solrQueryBuilder.Object, this.solrConfig.Object);
        }

        [Test]
        public void ShouldBatchQueriesToSolr()
        {
            var hashes = new List<QueryHash>();
            const int FingerprintsCount = 256;
            for (int i = 0; i < FingerprintsCount; ++i)
            {
                hashes.Add(new QueryHash(new int[25], (uint)i));
            }

            List<int> batchSizes = new List<int>();
            solrQueryBuilder.Setup(bld => bld.BuildReadQueryForHashesAndThreshold(It.IsAny<IEnumerable<int[]>>(), 5))
                                             .Returns("query")
                                             .Callback((IEnumerable<int[]> h, int t) => batchSizes.Add(h.Count()));
            solrQueryBuilder.Setup(bld => bld.BuildQueryForClusters(new[] { "CA" })).Returns("clusters:(CA)");
            solr.Setup(opr => opr.Query(It.IsAny<SolrQuery>(), It.IsAny<QueryOptions>()))
                .Returns(new SolrQueryResults<SubFingerprintDTO>())
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
            solrConfig.Setup(config => config.QueryBatchSize).Returns(BatchSize);
            solrConfig.Setup(config => config.PreferLocalShards).Returns(true);

            subFingerprintDao.ReadSubFingerprints(hashes, new DefaultQueryConfiguration { ThresholdVotes = 5, Clusters = new[] { "CA" }, });

            CollectionAssert.AreEqual(new List<int> { BatchSize, BatchSize, BatchSize, BatchSize, BatchSize, FingerprintsCount % BatchSize }, batchSizes);
        }

        [Test]
        public void ShouldSpecifyFilterQueries()
        {
            solrQueryBuilder.Setup(bld => bld.BuildReadQueryForHashesAndThreshold(It.IsAny<IEnumerable<int[]>>(), 5)).Returns("query");
            solrQueryBuilder.Setup(bld => bld.BuildQueryForClusters(It.IsAny<IEnumerable<string>>())).Returns("filter-query");

            solr.Setup(opr => opr.Query(It.IsAny<SolrQuery>(), It.IsAny<QueryOptions>()))
                .Returns(new SolrQueryResults<SubFingerprintDTO>())
                .Callback((SolrQuery q, QueryOptions opts) =>
                {
                    Assert.AreEqual("query", q.Query);
                    SolrQuery filter = (SolrQuery)opts.FilterQueries.First();
                    Assert.AreEqual("filter-query", filter.Query);
                }); 

            const int BatchSize = 50;
            solrConfig.Setup(config => config.QueryBatchSize).Returns(BatchSize);
            solrConfig.Setup(config => config.PreferLocalShards).Returns(true);

            subFingerprintDao.ReadSubFingerprints(new[] { new QueryHash(new int[25], 0) }, new DefaultQueryConfiguration { ThresholdVotes = 5, Clusters = new[] { "CA", "LA" } });
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
            dictionaryToHashConverter.Setup(dhc => dhc.FromSolrDictionaryToHashes(It.IsAny<IDictionary<int, int>>())).Returns(new int[0]);

            var subs = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference).ToList();

            Assert.AreEqual(1, subs.Count());
            CollectionAssert.AreEqual(new[] { "CA" }, subs.First().Clusters);
        }
    }
}
