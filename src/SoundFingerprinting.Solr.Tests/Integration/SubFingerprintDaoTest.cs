namespace SoundFingerprinting.Solr.Tests.Integration
{
    using System.Collections.Generic;
    using System.Linq;

    using Audio;

    using Builder;
    using Data;
    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;

    [TestFixture]
    public class SubFingerprintDaoTest : IntegrationTestWithSampleFiles
    {
        private readonly FingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();
        private readonly SolrModelService modelService = new SolrModelService();
        private readonly SubFingerprintDao subFingerprintDao = new SubFingerprintDao();
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
        public void ShouldInsertAndReadSubFingerprints()
        {
            var track = new TrackInfo("isrc", "artist", "title", 200);
            var trackData = trackDao.InsertTrack(track);
            const int NumberOfHashBins = 100;
            var genericHashBuckets = new int[25];
            var toInsert = Enumerable.Range(0, NumberOfHashBins).Select(sequenceNumber => new HashedFingerprint(
                                genericHashBuckets,
                                (uint)sequenceNumber,
                                sequenceNumber * 0.928f,
                                Enumerable.Empty<string>()))
                                .ToList();

            InsertHashedFingerprintsForTrack(toInsert, trackData.TrackReference);

            var read = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackData.TrackReference).ToList();

            Assert.AreEqual(NumberOfHashBins, read.Count);
            foreach (var hashedFingerprint in read)
            {
                CollectionAssert.AreEqual(genericHashBuckets, hashedFingerprint.Hashes);
            }
        }

        [Test]
        public void SameNumberOfHashBinsIsInsertedInAllTablesWhenFingerprintingEntireSongTest()
        {
            var track = new TrackInfo("id", "artist", "title", 197d);
            var trackData = trackDao.InsertTrack(track);

            var hashedFingerprints = HashFingerprintsForTrack(trackData.TrackReference);

            var hashes = subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackData.TrackReference).ToList();
            Assert.AreEqual(hashedFingerprints.Count, hashes.Count);
            foreach (var data in hashes)
            {
                Assert.AreEqual(25, data.Hashes.Length);
            }
        }

        [Test]
        public void ReadByTrackGroupIdWorksAsExpectedTest()
        {
            var firstTrack = new TrackInfo("id1", "first-artist", "title", 197d);
            var secondTrack = new TrackInfo("id2", "second-artist", "title", 197d);

            var firstTrackData = trackDao.InsertTrack(firstTrack);
            var secondTrackData = trackDao.InsertTrack(secondTrack);

            var hashedFingerprintsForFirstTrack = HashFingerprintsForTrack(firstTrackData.TrackReference, "first-group-id");
            var hashedFingerprintsForSecondTrack = HashFingerprintsForTrack(secondTrackData.TrackReference, "second-group-id");

            const int ThresholdVotes = 25;
            foreach (var hashedFingerprint in hashedFingerprintsForFirstTrack)
            {
                var subFingerprintData = subFingerprintDao.ReadSubFingerprints(new[] { new QueryHash(hashedFingerprint.HashBins, 0) },
                    new DefaultQueryConfiguration
                        {
                            ThresholdVotes = ThresholdVotes,
                            Clusters = new[] { "first-group-id" }
                        })
                    .Matches
                    .ToList();

                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(firstTrackData.TrackReference, subFingerprintData[0].SubFingerprint.TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(
                    new[] { new QueryHash(hashedFingerprint.HashBins, 0) },
                    new DefaultQueryConfiguration
                        {
                            ThresholdVotes = ThresholdVotes,
                            Clusters = new[] { "second-group-id" }
                        })
                    .Matches.ToList();

                Assert.AreEqual(1, subFingerprintData.Count);
                Assert.AreEqual(secondTrackData.TrackReference, subFingerprintData[0].SubFingerprint.TrackReference);

                subFingerprintData = subFingerprintDao.ReadSubFingerprints(new[] { new QueryHash(hashedFingerprint.HashBins, 0) },
                    new DefaultQueryConfiguration
                        {
                            ThresholdVotes = ThresholdVotes,
                            Clusters = new[]
                                           {
                                               "first-group-id", "second-group-id", "third-group-id"
                                           }
                        })
                    .Matches
                    .ToList();

                Assert.AreEqual(2, subFingerprintData.Count);
            }
        }

        [Test]
        public void ReadHashDataByTrackTest()
        {
            var firstTrack = new TrackInfo("id1", "artist", "title", 200);
            var firstTrackData = trackDao.InsertTrack(firstTrack);
            var firstHashData = HashFingerprintsForTrack(firstTrackData.TrackReference);
                
            var secondTrack = new TrackInfo("id2", "artist", "title", 200);
            var secondTrackData = trackDao.InsertTrack(secondTrack);
            var secondHashData = HashFingerprintsForTrack(secondTrackData.TrackReference);

            var resultFirstHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(firstTrackData.TrackReference).Select(
                    s => new HashedFingerprint(s.Hashes, s.SequenceNumber, s.SequenceAt, s.Clusters)).ToList();

            AssertHashDatasAreTheSame(firstHashData, resultFirstHashData);

            var resultSecondHashData = subFingerprintDao.ReadHashedFingerprintsByTrackReference(secondTrackData.TrackReference).Select(
                    s => new HashedFingerprint(s.Hashes, s.SequenceNumber, s.SequenceAt, s.Clusters)).ToList();

            AssertHashDatasAreTheSame(secondHashData, resultSecondHashData);
        }

        private void InsertHashedFingerprintsForTrack(IEnumerable<HashedFingerprint> hashedFingerprints, IModelReference trackReference)
        {
            subFingerprintDao.InsertHashDataForTrack(hashedFingerprints, trackReference);
        }

        private List<HashedFingerprint> HashFingerprintsForTrack(IModelReference firstTrackReference, params string[] clusters)
        {
            var hashedFingerprintsForFirstTrack =
                fingerprintCommandBuilder.BuildFingerprintCommand()
                    .From(GetAudioSamples())
                    .WithFingerprintConfig(
                        config =>
                        {
                            config.Clusters = clusters;
                            return config;
                        })
                    .UsingServices(audioService)
                    .Hash()
                    .Result;

            InsertHashedFingerprintsForTrack(hashedFingerprintsForFirstTrack, firstTrackReference);
            return hashedFingerprintsForFirstTrack;
        }
    }
}
