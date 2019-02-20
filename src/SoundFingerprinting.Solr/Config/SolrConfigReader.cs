namespace SoundFingerprinting.Solr.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;

    public static class SolrConfigReader
    {
        private static readonly IConfiguration configBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
        
        public static SoundFingerprintingSolrConfig GetSolrConfig()
        {
            var queryBatch = int.Parse(configBuilder["queryBatchSize"]);
            var preferLocalShards = bool.Parse(configBuilder["preferLocalShards"]);
            var config = new SoundFingerprintingSolrConfig(queryBatch, preferLocalShards);
            return config;
        }
        
        public static IEnumerable<SolrServer> GetServersFromLocalConfig()
        {
            IConfigurationSection solrConfig = configBuilder.GetSection("solr");
            
            return Enumerable.Empty<SolrServer>();
        }
    }
}