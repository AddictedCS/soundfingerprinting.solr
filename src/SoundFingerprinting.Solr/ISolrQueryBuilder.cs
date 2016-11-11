namespace SoundFingerprinting.Solr
{
    internal interface ISolrQueryBuilder
    {
        string BuildReadQueryForHashesAndThreshold(long[] hashBins, int thresholdVotes);

        string BuildReadQueryForTitleAndArtist(string title, string artist);
    }
}