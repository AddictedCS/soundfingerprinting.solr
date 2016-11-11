namespace SoundFingerprinting.Solr.Converters
{
    using System.Collections.Generic;
    using System.Linq;

    internal class DictionaryToHashConverter : IDictionaryToHashConverter
    {
        public Dictionary<int, long> FromHashesToSolrDictionary(long[] hashBins)
        {
            var hashTables = hashBins.Select((hash, index) => new { index, hash }).ToDictionary(
                x => x.index, x => x.hash);
            return hashTables;
        }

        public long[] FromSolrDictionaryToHashes(IDictionary<int, long> hashTables)
        {
            long[] hashBins = new long[hashTables.Count];
            foreach (var hashTable in hashTables)
            {
                hashBins[hashTable.Key] = hashTable.Value;
            }

            return hashBins;
        }
    }
}
