namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;
    using System.Linq;

    internal class SolrQueryBuilder
    {
        public string BuildReadQueryFor(long[] hashBins, int thresholdVotes)
        {
            List<string> termFreqs = hashBins.Select((hash, index) => string.Format("termfreq(hashTable_{0},'{1}')", index, hash)).ToList();
            string joined = string.Join(",", termFreqs);
            string queryString = string.Format("{{!frange l={0} u={1}}}sum({2})", thresholdVotes, hashBins.Length, joined);
            return queryString;
        }
    }
}
