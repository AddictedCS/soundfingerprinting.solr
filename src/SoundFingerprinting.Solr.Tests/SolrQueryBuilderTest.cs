namespace SoundFingerprinting.Solr.Tests
{
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class SolrQueryBuilderTest
    {
        private readonly SolrQueryBuilder queryBuilder = new SolrQueryBuilder();

        [Test]
        public void ShouldBuildReadWithThresholdsQuery()
        {
            int[] hashes = { 10, 11, 12, 21, 22 };

            string query = queryBuilder.BuildReadQueryForHashes(hashes);

            Assert.AreEqual("hashTable_0:10 hashTable_1:11 hashTable_2:12 hashTable_3:21 hashTable_4:22", query);
        }

        [Test]
        public void ShouldBuildArtistAndTitleQuery()
        {
            string query = queryBuilder.BuildReadQueryForTitleAndArtist("Piano Sonata No. 2 in \"B-flat minor\", Op. 35: III. Marche funèbre: Lento", "Chopin");

            Assert.AreEqual("title:\"Piano Sonata No. 2 in \\\"B-flat minor\\\", Op. 35: III. Marche funèbre: Lento\" AND artist:\"Chopin\"", query);
        }

        [Test]
        public void ShouldBuildQueryForClusters()
        {
            string query = queryBuilder.BuildQueryForClusters(new string[3] { "CA", "LA", "WA" });

            Assert.AreEqual("clusters:(\"CA\",\"LA\",\"WA\")", query);
        }

        [Test]
        public void ShouldBuildNestedQueryForMultipleFingerprints()
        {
            var fingerprints = new List<int[]> 
                {
                    new[] { 10, 11, 12, 21, 22 }, new[] { 12, 13, 14, 15, 16 } 
                };

            string query = queryBuilder.BuildReadQueryForHashesAndThreshold(fingerprints, 3);

            Assert.AreEqual(
                "_query_:\"{!edismax mm=3}hashTable_0:10 hashTable_1:11 hashTable_2:12 hashTable_3:21 hashTable_4:22\" _query_:\"{!edismax mm=3}hashTable_0:12 hashTable_1:13 hashTable_2:14 hashTable_3:15 hashTable_4:16\"",
                query);
        }
    }
}
