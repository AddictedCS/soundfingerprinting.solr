namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;
    using System.Linq;

    internal class SolrQueryBuilder : ISolrQueryBuilder
    {
        public string BuildReadQueryForHashes(long[] hashBins)
        {
            var terms = hashBins.Select((hash, index) => string.Format("hashTable_{0}:{1}", index, hash)).ToList();
            return string.Join(" ", terms);
        }

        public string BuildReadQueryForHashesAndThreshold(IEnumerable<long[]> allHashes, int thresholdVotes)
        {
            List<string> allSubQueries = new List<string>();
            foreach (var hash in allHashes)
            {
                var terms = hash.Select((h, index) => string.Format("hashTable_{0}:{1}", index, h)).ToList();
                string oneQuery = string.Format("_query_:\"{{!edismax mm={0}}}{1}\"", thresholdVotes, string.Join(" ", terms));
                allSubQueries.Add(oneQuery);
            }

            return string.Join(" OR ", allSubQueries);
        }

        public string BuildReadQueryForTitleAndArtist(string title, string artist)
        {
            string query = string.Format(
                "title:\"{0}\" AND artist:\"{1}\"", EscapeSolrQueryInput(title), EscapeSolrQueryInput(artist));
            return query;
        }

        private static string EscapeSolrQueryInput(string title)
        {
            return title.Replace("\"", "\\\"");
        }
    }
}
