namespace SoundFingerprinting.Solr.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    [Category("RequiresWindowsDLL")]
    [Category("IntegrationTest")]
    public class FingerprintCommandBuilderIntTest : IntegrationTestWithSampleFiles
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();
        private readonly IModelService modelService = new SolrModelService();
        private readonly IAudioService audioService = new NAudioService();
        private readonly ITagService tagService = new NAudioTagService();

        [TearDown]
        public void TearDown()
        {
            var allTracks = modelService.ReadAllTracks();
            foreach (var track in allTracks)
            {
                modelService.DeleteTrack(track.TrackReference);
            }
        }

        [Test]
        public void ShouldCreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var tags = tagService.GetTagInfo(PathToMp3);
            var track = new TrackData(tags);
            var trackReference = modelService.InsertTrack(track);

            var hashDatas = fingerprintCommandBuilder
                                            .BuildFingerprintCommand()
                                            .From(PathToMp3)
                                            .UsingServices(audioService)
                                            .Hash()
                                            .Result;

            modelService.InsertHashDataForTrack(hashDatas, trackReference);

            var queryResult = queryCommandBuilder.BuildQueryCommand()
                               .From(PathToMp3, SecondsToProcess, StartAtSecond)
                               .UsingServices(modelService, audioService)
                               .Query()
                               .Result;

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            Assert.AreEqual(trackReference, queryResult.BestMatch.Track.TrackReference);
            Assert.IsTrue(queryResult.BestMatch.QueryMatchLength > SecondsToProcess - 2);
            Assert.AreEqual(StartAtSecond, System.Math.Abs(queryResult.BestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(queryResult.BestMatch.Confidence > 0.8);
        }
    }
}
