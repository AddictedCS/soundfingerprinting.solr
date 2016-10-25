namespace SoundFingerprinting.Solr.DAO
{
    using System.Collections.Generic;

    using SolrNet.Attributes;

    public class SubFingerprintDTO
    {
        [SolrUniqueKey("subFingerprintId")]
        public string SubFingerprintId { get; set; }

        [SolrUniqueKey("trackId")]
        public string TrackId { get; set; }

        [SolrField("hashTable_")]
        public IDictionary<int, long> Hashes { get; set; }
    }
}
