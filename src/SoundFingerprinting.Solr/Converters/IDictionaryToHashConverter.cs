namespace SoundFingerprinting.Solr.Converters
{
    using System.Collections.Generic;

    internal interface IDictionaryToHashConverter
    {
        Dictionary<int, int> FromHashesToSolrDictionary(int[] hashBins);

        int[] FromSolrDictionaryToHashes(IDictionary<int, int> hashTables);
    }
}