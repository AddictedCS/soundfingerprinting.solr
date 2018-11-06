namespace SoundFingerprinting.Solr.Tests.Integration
{
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;

    [TestFixture]
    [Category("IntegrationTest")]
    public class SolrModelServiceTest : IntegrationTestWithSampleFiles
    {
        private readonly int[] firstTrackBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };

        private readonly int[] secondTrackBuckets = { 2, 2, 4, 5, 6, 7, 7, 9, 10, 11, 12, 13, 14, 14, 16, 17, 18, 19, 20, 20, 22, 23, 24, 25, 26 };

        // query buckets are similar with 5 elements from first track and 4 elements from second track
        private readonly int[] queryBuckets = { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };


        private readonly SolrModelService modelService = new SolrModelService();
        private readonly FingerprintCommandBuilder fcb = new FingerprintCommandBuilder();
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
        public async Task ShouldInsertAllSubFingerprintsForTrack()
        {
            var hashedFingerprints = await fcb.BuildFingerprintCommand()
                                         .From(GetAudioSamples())
                                         .UsingServices(audioService)
                                         .Hash();

            var track = new TrackInfo("id", "artist", "title", 4d);
            var reference = modelService.Insert(track, hashedFingerprints);

            var resultTrack = modelService.ReadTrackByReference(reference);
            Assert.AreEqual(track.Id, resultTrack.ISRC);
            Assert.AreEqual(track.Artist, resultTrack.Artist);
            Assert.AreEqual(track.Title, resultTrack.Title);
            Assert.AreEqual(track.DurationInSeconds, resultTrack.Length);
        }

        [Test]
        public void ShouldReadSubFingerprintsByHashBucketsHavingThreshold()
        {
            var firstTrack = new TrackInfo("id1", "artist", "title", 200);
            var secondTrack = new TrackInfo("id2", "artist", "title", 200);
            var firstHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, Enumerable.Empty<string>());
            var secondHashData = new HashedFingerprint(secondTrackBuckets, 1, 0.928f, Enumerable.Empty<string>());

            var firstTrackReference = modelService.Insert(firstTrack, new[] { firstHashData });
            var secondTrackReference = modelService.Insert(secondTrack, new[] { secondHashData });

            var subFingerprints = modelService.ReadSubFingerprints(new[] { queryBuckets }, new DefaultQueryConfiguration()).ToList();

            Assert.AreEqual(2, subFingerprints.Count);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
        }

        [Test]
        public void ShouldReadSubFingerprintsByHashBucketsHavingThresholdAndCluster()
        {
            var firstTrack = new TrackInfo("id1", "artist", "title", 200);
            var secondTrack = new TrackInfo("id2", "artist", "title", 200);
            var firstHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, new[] { "first-group-id" });
            var secondHashData = new HashedFingerprint(secondTrackBuckets, 1, 0.928f, new[] { "second-group-id" });

            var firstTrackReference = modelService.Insert(firstTrack, new[] { firstHashData });
            var secondTrackReference = modelService.Insert(secondTrack, new[] { secondHashData });

            var subFingerprints = modelService.ReadSubFingerprints(new[] { queryBuckets }, new DefaultQueryConfiguration { Clusters = new[] { "first-group-id" } }).ToList();

            Assert.AreEqual(1, subFingerprints.Count);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
        }

        [Test]
        public void ShouldReadSubFingerprintsByHashBucketsHavingThresholdAndMultipleClusters()
        {
            var firstTrack = new TrackInfo("id1", "artist", "title", 200);
            var secondTrack = new TrackInfo("isrc2", "artist", "title", 200);
            var firstHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, new[] { "first-group-id", "all", "hui" });
            var secondHashData = new HashedFingerprint(firstTrackBuckets, 1, 0.928f, new[] { "second-group-id", "all" });

            var firstTrackReference = modelService.Insert(firstTrack, new[] { firstHashData });
            var secondTrackReference = modelService.Insert(secondTrack, new[] { secondHashData });

            var subFingerprints = modelService.ReadSubFingerprints(new[] { queryBuckets }, new DefaultQueryConfiguration { Clusters = new[] { "not-all", "all" } }).ToList();

            Assert.AreEqual(2, subFingerprints.Count);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
            Assert.AreEqual(secondTrackReference, subFingerprints[1].TrackReference);
        }

        [Test]
        public async Task ShouldDeleteSubFingerprintsOnTrackDelete()
        {
            var track = new TrackInfo("id", "artist", "title", 3.3d);
            var audioSamples = GetAudioSamples();
            var hashData = await fcb.BuildFingerprintCommand()
                   .From(audioSamples)
                   .WithFingerprintConfig(config =>
                       {
                           config.Stride = new StaticStride(0);
                           return config;
                       })
                   .UsingServices(audioService)
                   .Hash();

            var trackReference = modelService.Insert(track, hashData);

            int modifiedRows = modelService.DeleteTrack(trackReference);

            Assert.AreEqual((int)(audioSamples.Duration / 1.48), modifiedRows);
        }
    }
}
