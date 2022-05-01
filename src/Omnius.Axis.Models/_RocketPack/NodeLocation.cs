namespace Omnius.Axis.Models;

public sealed partial class NodeLocation
{
    public override string ToString()
    {
        return string.Join(",", this.Addresses);
    }
}
