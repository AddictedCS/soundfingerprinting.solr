namespace SoundFingerprinting.Solr.Config
{
    public class SoundFingerprintingSolrConfig : ISoundFingerprintingSolrConfig
    {
        public SoundFingerprintingSolrConfig(int queryBatchSize, bool preferLocalShards)
        {
            QueryBatchSize = queryBatchSize;
            PreferLocalShards = preferLocalShards;
        }

        public int QueryBatchSize { get; }

        public bool PreferLocalShards { get; }
    }
}
