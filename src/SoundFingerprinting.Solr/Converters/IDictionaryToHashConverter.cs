namespace SoundFingerprinting.Solr.Converters
{
    using System.Collections.Generic;

    internal interface IDictionaryToHashConverter
    {
        Dictionary<int, long> FromHashesToSolrDictionary(long[] hashBins);

        long[] FromSolrDictionaryToHashes(IDictionary<int, long> hashTables);
    }
}