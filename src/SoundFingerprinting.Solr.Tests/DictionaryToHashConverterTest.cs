namespace SoundFingerprinting.Solr.Tests
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using SoundFingerprinting.Solr.Converters;

    [TestFixture]
    public class DictionaryToHashConverterTest
    {
        private readonly DictionaryToHashConverter dictionaryToHashConverter = new DictionaryToHashConverter();

        [Test]
        public void ShouldConvertToAndFrom()
        {
            int[] hashBins = { 3, 4, 5, 7, 8 };
            Dictionary<int, int> hashTables = dictionaryToHashConverter.FromHashesToSolrDictionary(hashBins);

            Assert.AreEqual(3, hashTables[0]);
            Assert.AreEqual(4, hashTables[1]);
            Assert.AreEqual(5, hashTables[2]);
            Assert.AreEqual(7, hashTables[3]);
            Assert.AreEqual(8, hashTables[4]);

            int[] actual = dictionaryToHashConverter.FromSolrDictionaryToHashes(hashTables);

            CollectionAssert.AreEqual(hashBins, actual);
        }
    }
}
