namespace Omnius.Axus.Messages;

public sealed partial class NodeLocation
{
    public override string ToString()
    {
        return string.Join(",", this.Addresses);
    }
}
