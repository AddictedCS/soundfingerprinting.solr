namespace SoundFingerprinting.Solr.Config
{
    using System.Collections.Generic;

    public class SoundFingerprintingSolrConfiguration
    {
        public int QueryBatchSize { get; set; }
        public bool PreferLocalShards { get; set; }
        public List<SolrServer> Servers { get; set; }
    }
}