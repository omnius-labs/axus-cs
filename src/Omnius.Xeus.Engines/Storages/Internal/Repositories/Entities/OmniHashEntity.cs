using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class OmniHashEntity
    {
        public int AlgorithmType { get; set; }
        public byte[]? Value { get; set; }

        public static OmniHashEntity Import(OmniHash value)
        {
            return new OmniHashEntity() { AlgorithmType = (int)value.AlgorithmType, Value = value.Value.ToArray() };
        }

        public OmniHash Export()
        {
            return new OmniHash((OmniHashAlgorithmType)this.AlgorithmType, this.Value);
        }
    }
}
