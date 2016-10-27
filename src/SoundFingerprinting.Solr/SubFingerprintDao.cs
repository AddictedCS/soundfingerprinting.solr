namespace SoundFingerprinting.Solr
{
    using System;
    using System.Linq;

    using SolrNet;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Solr.DAO;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private readonly ISolrOperations<SubFingerprintDTO> solr;

        public SubFingerprintDao() : this(DependencyResolver.Current.Get<ISolrOperations<SubFingerprintDTO>>())
        {
        }

        protected SubFingerprintDao(ISolrOperations<SubFingerprintDTO> solr)
        {
            this.solr = solr;
        }

        public SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference)
        {
            throw new NotImplementedException();
        }

        public IModelReference InsertSubFingerprint(byte[] signature, int sequenceNumber, double sequenceAt, IModelReference trackReference)
        {
            Guid subId = Guid.NewGuid();

            // Ignore byte signature. We will use it only in HashBinDao class!
            var dto = new SubFingerprintDTO
                {
                    SubFingerprintId = subId.ToString(),
                    SequenceAt = sequenceAt,
                    SequenceNumber = sequenceNumber,
                    TrackId = SolrModelReference.GetId(trackReference)
                };

            this.solr.Add(dto);
            this.solr.Commit();
            return new SolrModelReference(subId.ToString());
        }

        
    }
}
