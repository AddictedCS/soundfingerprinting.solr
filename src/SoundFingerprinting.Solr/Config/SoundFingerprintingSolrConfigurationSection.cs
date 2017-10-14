namespace SoundFingerprinting.Solr.Config
{
    using System.Configuration;

    using Ninject.Integration.SolrNet.Config;

    public class SoundFingerprintingSolrConfigurationSection : SolrConfigurationSection
    {
        [ConfigurationProperty("queryBatchSize", DefaultValue = 100)]
        public int QueryBatchSize
        {
            get
            {
                return (int)this["queryBatchSize"];
            }

            set
            {
                this["queryBatchSize"] = value;
            }
        }

        [ConfigurationProperty("preferLocalShards", DefaultValue = false)]
        public bool PreferLocalShards
        {
            get
            {
                return (bool)this["preferLocalShards"];
            }

            set
            {
                this["preferLocalShards"] = value;
            }
        }
    }
}
