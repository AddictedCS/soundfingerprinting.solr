namespace SoundFingerprinting.Solr.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;

    public static class SolrConfigReader
    {
        private const string AppSettings = "appsettings.json";
        
        private static readonly IConfiguration ConfigBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(AppSettings, optional: false, reloadOnChange: false)
            .Build();
        
        public static SoundFingerprintingSolrConfig GetSolrConfig()
        {
            var queryBatch = int.Parse(ConfigBuilder["queryBatchSize"]);
            var preferLocalShards = bool.Parse(ConfigBuilder["preferLocalShards"]);
            var config = new SoundFingerprintingSolrConfig(queryBatch, preferLocalShards);
            return config;
        }
        
        public static IEnumerable<SolrServer> GetServersFromLocalConfig()
        {
            var solrConfig = ConfigBuilder.GetSection("solr");

            return solrConfig.GetChildren()
                .Select(config =>
                {
                    string id = config["id"];
                    string url = config["url"];
                    string documentType = config["documentType"];
                    return new SolrServer(id, url, documentType);
                });
        }
    }
}