namespace SoundFingerprinting.Solr.DAO
{
    using System.Collections.Generic;

    using SolrNet.Attributes;

    internal class SubFingerprintDTO
    {
        [SolrUniqueKey("subFingerprintId")]
        public string SubFingerprintId { get; set; }

        [SolrField("trackId")]
        public string TrackId { get; set; }

        [SolrField("sequenceNumber")]
        public int SequenceNumber { get; set; }

        [SolrField("sequenceAt")]
        public double SequenceAt { get; set; }

        [SolrField("hashTable_")]
        public IDictionary<int, long> Hashes { get; set; }
    }
}
