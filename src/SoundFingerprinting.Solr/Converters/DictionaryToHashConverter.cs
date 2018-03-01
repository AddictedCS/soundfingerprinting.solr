namespace SoundFingerprinting.Solr.Converters
{
    using System.Collections.Generic;
    using System.Linq;

    internal class DictionaryToHashConverter : IDictionaryToHashConverter
    {
        public Dictionary<int, int> FromHashesToSolrDictionary(int[] hashBins)
        {
            var hashTables = hashBins.Select((hash, index) => new { index, hash })
                                     .ToDictionary(x => x.index, x => x.hash);
            return hashTables;
        }

        public int[] FromSolrDictionaryToHashes(IDictionary<int, int> hashTables)
        {
            int[] hashBins = new int[hashTables.Count];
            foreach (var hashTable in hashTables)
            {
                hashBins[hashTable.Key] = hashTable.Value;
            }

            return hashBins;
        }
    }
}
