namespace SoundFingerprinting.Solr
{
    using System;

    using Microsoft.Practices.ServiceLocation;

    using SolrNet;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.DAO;

    public class SubFingerprintDao : ISubFingerprintDao
    {
        private ISolrOperations<SubFingerprintDTO> solr;

        public SubFingerprintDao()
        {
            this.solr = DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>();
        }

        public SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference)
        {
            throw new NotImplementedException();
        }

        public IModelReference InsertSubFingerprint(byte[] signature, int sequenceNumber, double sequenceAt, IModelReference trackReference)
        {
            Guid subId = Guid.NewGuid();

            // Ignore byte signature. We will use it only in HashBinDao class!
            var dto = new SubFingerprintDTO()
                {
                    SubFingerprintId = subId.ToString(),
                    SequenceAt = sequenceAt,
                    SequenceNumber = sequenceNumber,
                    TrackId = ((SolrModelReference)trackReference).Id
                };

            this.solr.Add(dto);
            this.solr.Commit();
            return new SolrModelReference(subId.ToString());
        }
    }
}
