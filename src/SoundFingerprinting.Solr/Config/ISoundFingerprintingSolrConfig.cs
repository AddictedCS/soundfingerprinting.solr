namespace SoundFingerprinting.Solr.Config
{
    internal interface ISoundFingerprintingSolrConfig
    {
        int QueryBatchSize { get; }

        bool PreferLocalShards { get; }
    }
}