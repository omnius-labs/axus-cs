using System.Text;

namespace Omnius.Axis.Models;

public sealed partial class NodeLocation
{
    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var a in this.Addresses)
        {
            sb.AppendLine(a.ToString());
        }

        return sb.ToString();
    }
}
