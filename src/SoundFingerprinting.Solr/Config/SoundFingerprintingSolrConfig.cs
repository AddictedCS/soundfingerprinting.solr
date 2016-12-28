namespace SoundFingerprinting.Solr.Config
{
    internal class SoundFingerprintingSolrConfig : ISoundFingerprintingSolrConfig
    {
        public SoundFingerprintingSolrConfig(int queryBatchSize, bool preferLocalShards)
        {
            QueryBatchSize = queryBatchSize;
            PreferLocalShards = preferLocalShards;
        }

        public int QueryBatchSize { get; private set; }

        public bool PreferLocalShards { get; private set; }
    }
}
