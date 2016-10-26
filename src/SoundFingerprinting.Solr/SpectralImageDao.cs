namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    internal class SpectralImageDao : ISpectralImageDao
    {
        public void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference)
        {
            throw new NotImplementedException();
        }

        public List<SpectralImageData> GetSpectralImagesByTrackId(IModelReference trackReference)
        {
            throw new NotImplementedException();
        }
    }
}
