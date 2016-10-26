namespace SoundFingerprinting.Solr
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    internal class FingerprintDao : IFingerprintDao
    {
        public IModelReference InsertFingerprint(FingerprintData fingerprint)
        {
            throw new NotImplementedException("Solr does not support fingerprint storage");
        }

        public IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            throw new NotImplementedException("Solr does not suport fingerprint storage");
        }
    }
}
