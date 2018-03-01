namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;
    using System.Linq;

    internal class SolrQueryBuilder : ISolrQueryBuilder
    {
        public string BuildReadQueryForHashes(int[] hashBins)
        {
            var terms = hashBins.Select((hash, index) => $"hashTable_{index}:{hash}").ToList();
            return string.Join(" ", terms);
        }

        public string BuildReadQueryForHashesAndThreshold(IEnumerable<int[]> allHashes, int thresholdVotes)
        {
            List<string> allSubQueries = new List<string>();
            foreach (var hash in allHashes)
            {
                var terms = hash.Select((h, index) => $"hashTable_{index}:{h}").ToList();
                string oneQuery = $"_query_:\"{{!edismax mm={thresholdVotes}}}{string.Join(" ", terms)}\"";
                allSubQueries.Add(oneQuery);
            }

            return string.Join(" ", allSubQueries);
        }

        public string BuildReadQueryForTitleAndArtist(string title, string artist)
        {
            string query = $"title:\"{EscapeSolrQueryInput(title)}\" AND artist:\"{EscapeSolrQueryInput(artist)}\"";
            return query;
        }

        public string BuildQueryForClusters(IEnumerable<string> clusters)
        {
            return
                $"clusters:({string.Join(",", clusters.Select(cluster => $"\"{EscapeSolrQueryInput(cluster)}\""))})";
        }

        private static string EscapeSolrQueryInput(string title)
        {
            return title.Replace("\"", "\\\"");
        }
    }
}
