namespace SoundFingerprinting.Solr.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    [Category("RequiresWindowsDLL")]
    [Category("IntegrationTest")]
    public class FingerprintCommandBuilderIntTest : IntegrationTestWithSampleFiles
    {
        private readonly FingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly QueryFingerprintService queryFingerprintService = new QueryFingerprintService();
        private readonly SolrModelService modelService = new SolrModelService();
        private readonly NAudioService audioService = new NAudioService();

        [Test]
        public void ShouldCreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var track = new TrackData("isrc-solr", "artist", "title", "album", 1986, 4);
            var trackReference = modelService.InsertTrack(track);

            var hashDatas = fingerprintCommandBuilder
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .UsingServices(audioService)
                                            .Hash()
                                            .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var defaultQueryConfiguration = new DefaultQueryConfiguration();
            
            var queryResult = queryFingerprintService.Query(hashDatas, defaultQueryConfiguration, modelService);

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            Assert.AreEqual(trackReference, queryResult.BestMatch.Track.TrackReference);
        }
    }
}
