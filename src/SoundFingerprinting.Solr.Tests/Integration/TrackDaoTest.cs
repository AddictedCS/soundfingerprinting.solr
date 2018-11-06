namespace SoundFingerprinting.Solr.Tests.Integration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    [TestFixture]
    [Category("IntegrationTest")]
    public class TrackDaoTest
    {
        private readonly SolrModelService modelService = new SolrModelService();
        private readonly TrackDao trackDao = new TrackDao();

        [TearDown]
        public void TearDown()
        {
            var allTracks = modelService.ReadAllTracks();
            TearDownTracks(allTracks.Select(t => t.TrackReference));
        }

        [Test]
        public void ShouldInsertTrack()
        {
            var track = new TrackInfo("id", "artist", "title", 3.6);
            var trackData = trackDao.InsertTrack(track);

            Assert.IsNotNull(trackData.TrackReference);
        }

        [Test]
        public void ShouldReadTrack()
        {
            var expected = new TrackInfo("isrc", "artist", "title", 4.0);
            var trackData = trackDao.InsertTrack(expected);

            var actual = trackDao.ReadTrack(trackData.TrackReference);

            AssertTracksAreEqual(trackData, actual);
        }

        [Test]
        public void ShouldReadMultipleTracks()
        {
            var refs = new List<IModelReference>();
            var l = 10;
            for (int i = 0; i < l; ++i)
            {
                var track = new TrackInfo($"id_{i}", "artist", "title", 100);
                var trackData = trackDao.InsertTrack(track);
                refs.Add(trackData.TrackReference);
            }

            var actual = trackDao.ReadTracksByReferences(refs);
            Assert.AreEqual(10, actual.Count());
        }

        [Test]
        public void ShouldReadNonExistingTrack()
        {
            var actual = trackDao.ReadTrack(new SolrModelReference("256-256"));

            Assert.IsNull(actual);
        }

        [Test]
        public void ShouldDeleteTrack()
        {
            var expected = new TrackInfo("id", "artist", "title", 4.0);
            var trackData = trackDao.InsertTrack(expected);
            var actual = trackDao.ReadTrack(trackData.TrackReference);

            AssertTracksAreEqual(trackData, actual);

            trackDao.DeleteTrack(trackData.TrackReference);

            var tracks = trackDao.ReadAll();
            Assert.AreEqual(0, tracks.Count());
        }

        [Test]
        public void ShouldInsertMultipleTracksConcurrently()
        {
            const int NumberOfTracks = 100;
            var modelReferences = new ConcurrentBag<IModelReference>();
            for (int i = 0; i < NumberOfTracks; i++)
            {
                var trackData = trackDao.InsertTrack(new TrackInfo("id", "artist", "title", 200));
                Assert.IsFalse(modelReferences.Contains(trackData.TrackReference));
                modelReferences.Add(trackData.TrackReference);
            }

            Assert.AreEqual(NumberOfTracks, trackDao.ReadAll().Count());
        }

        [Test]
        public void ShouldReadAllTracksFromTheCore()
        {
            const int TrackCount = 5;
            var expectedTracks = InsertRandomTracks(TrackCount);

            var tracks = trackDao.ReadAll().ToList();

            Assert.AreEqual(TrackCount, tracks.Count);
            foreach (var expectedTrack in expectedTracks)
            {
                Assert.IsTrue(tracks.Any(track => track.ISRC == expectedTrack.ISRC));
            }
        }

        [Test]
        public void ShouldInsertMultipleTracksViaOneCall()
        {
            const int TrackCount = 100;
            var tracks = InsertRandomTracks(TrackCount);

            var actualTracks = trackDao.ReadAll().ToList();

            Assert.AreEqual(tracks.Count, actualTracks.Count);
            for (int i = 0; i < actualTracks.Count; i++)
            {
                AssertTracksAreEqual(
                    tracks[i], actualTracks.First(track => track.TrackReference.Equals(tracks[i].TrackReference)));
            }
        }

        [Test]
        public void ReadTrackByArtistAndTitleTest()
        {
            var track = new TrackInfo("id1", "some title", "artist", 120d);
            trackDao.InsertTrack(track);

            var tracks = trackDao.ReadTrackByTitle(track.Title).ToList();

            Assert.IsNotNull(tracks);
            Assert.IsTrue(tracks.Count == 1);
            Assert.AreEqual(track.Id, tracks[0].ISRC);
        }

        [Test]
        public void ReadByNonExistentArtistAndTitleTest()
        {
            var tracks = trackDao.ReadTrackByTitle("title");

            Assert.IsFalse(tracks.Any());
        }

        [Test]
        public void ReadTrackByISRCTest()
        {
            var expectedTrack = GetRandomTrack();
            trackDao.InsertTrack(expectedTrack);

            var actualTrack = trackDao.ReadTrackById(expectedTrack.Id);

            Assert.AreEqual(expectedTrack.Id, actualTrack.ISRC);
        }

        [Test]
        public void DeleteCollectionOfTracksTest()
        {
            const int NumberOfTracks = 10;
            InsertRandomTracks(NumberOfTracks);

            var allTracks = trackDao.ReadAll().ToList();

            Assert.IsTrue(allTracks.Count == NumberOfTracks);
            foreach (var track in allTracks)
            {
                trackDao.DeleteTrack(track.TrackReference);
            }

            Assert.IsFalse(trackDao.ReadAll().Any());
        }

        [Test]
        public void InsertTrackShouldAcceptEmptyEntriesCodes()
        {
            var track = new TrackInfo(string.Empty, string.Empty, string.Empty, 200);
            var trackReference = trackDao.InsertTrack(track);

            var actualTrack = trackDao.ReadTrack(trackReference.TrackReference);

            Assert.AreEqual(track.Id, actualTrack.ISRC);
        }

        private void AssertTracksAreEqual(TrackData expectedTrack, TrackData actualTrack)
        {
            Assert.AreEqual(expectedTrack.Artist, actualTrack.Artist);
            Assert.AreEqual(expectedTrack.Title, actualTrack.Title);
            Assert.AreEqual(expectedTrack.Length, actualTrack.Length);
            Assert.AreEqual(expectedTrack.ISRC, actualTrack.ISRC);
        }

        private List<TrackData> InsertRandomTracks(int trackCount)
        {
            var tracks = new List<TrackData>();
            for (int i = 0; i < trackCount; i++)
            {
                var track = GetRandomTrack();
                var trackData = trackDao.InsertTrack(track);
                tracks.Add(trackData);
            }

            return tracks;
        }

        private TrackInfo GetRandomTrack()
        {
            return new TrackInfo(Guid.NewGuid().ToString(), "artist", "title", 360);
        }

        private void TearDownTracks(IEnumerable<IModelReference> modelReferences)
        {
            foreach (var modelReference in modelReferences)
            {
                modelService.DeleteTrack(modelReference);
            }
        }
    }
}
