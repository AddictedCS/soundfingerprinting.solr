namespace SoundFingerprinting.Solr.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class SolrModelServiceIntTest : IntegrationTestWithSampleFiles
    {
        private readonly long[] firstTrackBuckets = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };

        private readonly long[] secondTrackBuckets = new long[] { 2, 2, 4, 5, 6, 7, 7, 9, 10, 11, 12, 13, 14, 14, 16, 17, 18, 19, 20, 20, 22, 23, 24, 25, 26 };

        // query buckets are similar with 5 elements from first track and 4 elements from second track
        private readonly long[] queryBuckets = new long[] { 3, 2, 5, 6, 7, 8, 7, 10, 11, 12, 13, 14, 15, 14, 17, 18, 19, 20, 21, 20, 23, 24, 25, 26, 25 };


        private readonly SolrModelService modelService = new SolrModelService();
        private readonly FingerprintCommandBuilder fcb = new FingerprintCommandBuilder();
        private readonly NAudioService audioService = new NAudioService();
        private readonly TrackDao trackDao = new TrackDao();

        [TearDown]
        public void TearDown()
        {
            var allTracks = trackDao.ReadAll();
            foreach (var track in allTracks)
            {
                trackDao.DeleteTrack(track.TrackReference);
            }
        }

        [Test]
        public void ShouldInsertAllSubfingerprintsForTrack()
        {
            var hashedFingerprints = fcb.BuildFingerprintCommand()
                                         .From(GetAudioSamples())
                                         .UsingServices(audioService)
                                         .Hash()
                                         .Result;

            var track = new TrackData("isrc", "artist", "title", "album", 1986, 4d);
            var trackReference = modelService.InsertTrack(track);

            modelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
        }

        [Test]
        public void ShouldReadSubFingerprintsByHashBucketsHavingThreshold()
        {
            var firstTrack = new TrackData("isrc1", "artist", "title", "album", 1986, 200);
            var firstTrackReference = modelService.InsertTrack(firstTrack);
            var secondTrack = new TrackData("isrc2", "artist", "title", "album", 1986, 200);
            var secondTrackReference = modelService.InsertTrack(secondTrack);
            var firstHashData = new HashedFingerprint(GenericSignature(), firstTrackBuckets, 1, 0.928, Enumerable.Empty<string>());
            var secondHashData = new HashedFingerprint(GenericSignature(), secondTrackBuckets, 1, 0.928, Enumerable.Empty<string>());

            modelService.InsertHashDataForTrack(new[] { firstHashData }, firstTrackReference);
            modelService.InsertHashDataForTrack(new[] { secondHashData }, secondTrackReference);

            var subFingerprints = modelService.ReadSubFingerprints(queryBuckets, new DefaultQueryConfiguration());

            Assert.AreEqual(1, subFingerprints.Count);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
        }

        [Test]
        public void ShouldReadSubFingerprintsByHashBucketsHavingThresholdAndCluster()
        {
            var firstTrack = new TrackData("isrc1", "artist", "title", "album", 1986, 200);
            var firstTrackReference = modelService.InsertTrack(firstTrack);
            var secondTrack = new TrackData("isrc2", "artist", "title", "album", 1986, 200);
            var secondTrackReference = modelService.InsertTrack(secondTrack);
            var firstHashData = new HashedFingerprint(GenericSignature(), firstTrackBuckets, 1, 0.928, new[] { "first-group-id" });
            var secondHashData = new HashedFingerprint(GenericSignature(), secondTrackBuckets, 1, 0.928, new[] { "second-group-id" });

            modelService.InsertHashDataForTrack(new[] { firstHashData }, firstTrackReference);
            modelService.InsertHashDataForTrack(new[] { secondHashData }, secondTrackReference);

            var subFingerprints = modelService.ReadSubFingerprints(queryBuckets, new DefaultQueryConfiguration { Clusters = new[] { "first-group-id" } });

            Assert.AreEqual(1, subFingerprints.Count);
            Assert.AreEqual(firstTrackReference, subFingerprints[0].TrackReference);
        }

        [Test]
        public void ShouldDeleteSubfingerprintsOnTrackDelete()
        {
            var track = new TrackData("isrc", "artist", "title", "album", 1986, 3.3d);
            var trackReference = trackDao.InsertTrack(track);
            var audioSamples = GetAudioSamples();
            var hashData = fcb.BuildFingerprintCommand()
                   .From(audioSamples)
                   .WithFingerprintConfig(config =>
                       {
                           config.Stride = new StaticStride(0);
                       })
                   .UsingServices(this.audioService)
                   .Hash()
                   .Result;

            modelService.InsertHashDataForTrack(hashData, trackReference);

            int modifiedRows = trackDao.DeleteTrack(trackReference);

            Assert.AreEqual((int)(audioSamples.Duration / 1.48), modifiedRows);
        }
    }
}
