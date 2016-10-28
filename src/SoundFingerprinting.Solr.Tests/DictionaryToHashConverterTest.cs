namespace SoundFingerprinting.Solr.Tests
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DictionaryToHashConverterTest
    {
        private readonly DictionaryToHashConverter dictionaryToHashConverter = new DictionaryToHashConverter();

        [TestMethod]
        public void ShouldConvertToAndFrom()
        {
            long[] hashBins = new long[] { 3, 4, 5, 7, 8 };
            Dictionary<int, long> hashTables = dictionaryToHashConverter.FromHashes(hashBins);

            Assert.AreEqual(3, hashTables[0]);
            Assert.AreEqual(4, hashTables[1]);
            Assert.AreEqual(5, hashTables[2]);
            Assert.AreEqual(7, hashTables[3]);
            Assert.AreEqual(8, hashTables[4]);

            long[] actual = dictionaryToHashConverter.FromSolrDictionary(hashTables);
            for (int i = 0; i < hashBins.Length; ++i)
            {
                Assert.AreEqual(hashBins[i], actual[i]);
            }
        }
    }
}
