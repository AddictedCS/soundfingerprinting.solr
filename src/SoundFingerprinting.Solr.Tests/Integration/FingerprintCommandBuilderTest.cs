namespace SoundFingerprinting.Solr.Tests.Integration
{
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Data;

    [TestFixture]
    [Category("IntegrationTest")]
    public class FingerprintCommandBuilderTest : IntegrationTestWithSampleFiles
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();
        private readonly IModelService modelService = new SolrModelService();
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();

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
        public async Task ShouldCreateFingerprintsInsertThenQueryAndGetTheRightResult()
        {
            const int SecondsToProcess = 10;
            const int StartAtSecond = 30;
            var audioSamples = GetAudioSamples();
            var track = new TrackInfo("id", audioSamples.Origin, audioSamples.Origin, audioSamples.Duration);
            var fingerprints = await fingerprintCommandBuilder.BuildFingerprintCommand()
                    .From(audioSamples)
                    .UsingServices(audioService)
                    .Hash();

            var trackReference = modelService.Insert(track, fingerprints);

            var querySamples = GetQuerySamples(audioSamples, StartAtSecond, SecondsToProcess);

            var queryResult = await queryCommandBuilder.BuildQueryCommand()
                    .From(new AudioSamples(querySamples, string.Empty, audioSamples.SampleRate))
                    .UsingServices(modelService, audioService)
                    .Query();

            Assert.IsTrue(queryResult.ContainsMatches);
            Assert.AreEqual(1, queryResult.ResultEntries.Count());
            var bestMatch = queryResult.BestMatch;
            Assert.AreEqual(trackReference, bestMatch.Track.TrackReference);
            Assert.IsTrue(bestMatch.QueryMatchLength > SecondsToProcess - 3, $"QueryMatchLength:{bestMatch.QueryLength}");
            Assert.AreEqual(StartAtSecond, System.Math.Abs(bestMatch.TrackStartsAt), 0.1d);
            Assert.IsTrue(bestMatch.Confidence > 0.7, $"Confidence:{bestMatch.Confidence}");
        }
    }
}
