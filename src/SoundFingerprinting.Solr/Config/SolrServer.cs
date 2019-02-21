namespace SoundFingerprinting.Solr.Config
{
    using Ninject.Integration.SolrNet;

    public class SolrServer : ISolrServer
    {
        public SolrServer(string id, string url, string documentType)
        {
            Id = id;
            Url = url;
            DocumentType = documentType;
        }
        
        public string Id { get; set; }
        public string Url { get; set; }
        public string DocumentType { get; set; }
    }
}