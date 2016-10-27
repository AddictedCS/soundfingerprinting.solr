namespace SoundFingerprinting.Solr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SolrQueryBuilderTest
    {
        private readonly SolrQueryBuilder solrQueryBuilder = new SolrQueryBuilder();

        [TestMethod]
        public void ShouldBuildReadWithThresholdsQuery()
        {
            long[] hashes = new long[] { 10, 11, 12, 21, 22 };
            const int Threshold = 3;

            string query = solrQueryBuilder.BuildReadQueryFor(hashes, Threshold);

            Assert.AreEqual("{!frange l=3 u=5}sum(termfreq(hashTable_0,'10'),termfreq(hashTable_1,'11'),termfreq(hashTable_2,'12'),termfreq(hashTable_3,'21'),termfreq(hashTable_4,'22'))", query);
        }
    }
}
