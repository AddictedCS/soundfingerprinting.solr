namespace SoundFingerprinting.Solr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SolrQueryBuilderTest
    {
        private readonly SolrQueryBuilder qb = new SolrQueryBuilder();

        [TestMethod]
        public void ShouldBuildReadWithThresholdsQuery()
        {
            long[] hashes = new long[] { 10, 11, 12, 21, 22 };
            const int Threshold = 3;

            string query = qb.BuildReadQueryForHashesAndThreshold(hashes, Threshold);

            Assert.AreEqual("hashTable_0:10 hashTable_1:11 hashTable_2:12 hashTable_3:21 hashTable_4:22", query);
        }

        [TestMethod]
        public void ShouldBuildArtistAndTitleQuery()
        {
            string query =
                qb.BuildReadQueryForTitleAndArtist(
                    "Piano Sonata No. 2 in \"B-flat minor\", Op. 35: III. Marche funèbre: Lento", "Chopin");

            Assert.AreEqual("title:\"Piano Sonata No. 2 in \\\"B-flat minor\\\", Op. 35: III. Marche funèbre: Lento\" AND artist:\"Chopin\"", query);
        }
    }
}
