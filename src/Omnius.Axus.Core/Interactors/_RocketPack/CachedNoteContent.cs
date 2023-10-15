namespace Omnius.Axus.Interactors.Internal.Models;

public partial class CachedNoteBox
{
    public IEnumerable<CachedMemo> ToMemos()
    {
        var results = new List<CachedMemo>();

        foreach (var note in this.Value.Notes)
        {
            var result = new CachedMemo(this.Signature, note);
            results.Add(result);
        }

        return results;
    }
}
