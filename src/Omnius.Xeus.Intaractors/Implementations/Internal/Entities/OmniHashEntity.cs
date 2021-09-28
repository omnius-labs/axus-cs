using LiteDB;

namespace Omnius.Xeus.Intaractors.Internal.Entities
{
    internal record OmniHashEntity
    {
        [BsonCtor]
        public OmniHashEntity(int algorithmType, byte[] value)
        {
            this.AlgorithmType = algorithmType;
            this.Value = value;
        }

        public int AlgorithmType { get; }

        public byte[] Value { get; }
    }
}
