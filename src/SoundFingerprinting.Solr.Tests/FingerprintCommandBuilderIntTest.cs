namespace SoundFingerprinting.Solr.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;

    [TestClass]
    public class FingerprintCommandBuilderIntTest : IntegrationTestWithSampleFiles
    {
        private readonly FingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly QueryFingerprintService queryFingerprintService = new QueryFingerprintService();
        private readonly SolrModelService modelService = new SolrModelService();
        private readonly NAudioService audioService = new NAudioService();

        [TestMethod]
        public void ShouldCreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var track = new TrackData("isrc-solr", "artist", "title", "album", 1986, 4);
            var trackReference = modelService.InsertTrack(track);

            var hashDatas = fingerprintCommandBuilder
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3, SecondsToProcess, StartAtSecond)
                                            .UsingServices(this.audioService)
                                            .Hash()
                                            .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var queryResult = queryFingerprintService.QueryWithTimeSequenceInformation(modelService, hashDatas, new DefaultQueryConfiguration());

            Assert.IsTrue(queryResult.IsSuccessful);
            Assert.AreEqual(1, queryResult.ResultEntries.Count);
            Assert.AreEqual(trackReference, queryResult.ResultEntries[0].Track.TrackReference);
        }
    }
}
