namespace SoundFingerprinting.Solr
{
    using System.Collections.Generic;

    internal interface ISolrQueryBuilder
    {
        string BuildReadQueryForHashes(int[] hashBins);

        string BuildReadQueryForHashesAndThreshold(IEnumerable<int[]> allHashes, int thresholdVotes);

        string BuildReadQueryForTitle(string title);

        string BuildQueryForClusters(IEnumerable<string> clusters);
    }
}