namespace SoundFingerprinting.Solr.Config
{
    using Ninject.Integration.SolrNet;

    public class SolrServer : ISolrServer
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string DocumentType { get; set; }
    }
}