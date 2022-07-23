using Omnius.Core.Cryptography;

namespace Omnius.Axis.Interactors.Internal.Entities;

internal record OmniHashEntity
{
    public int AlgorithmType { get; set; }

    public byte[]? Value { get; set; }

    public static OmniHashEntity Import(OmniHash item)
    {
        return new OmniHashEntity()
        {
            AlgorithmType = (int)item.AlgorithmType,
            Value = item.Value.ToArray(),
        };
    }

    public OmniHash Export()
    {
        return new OmniHash((OmniHashAlgorithmType)this.AlgorithmType, this.Value);
    }
}
