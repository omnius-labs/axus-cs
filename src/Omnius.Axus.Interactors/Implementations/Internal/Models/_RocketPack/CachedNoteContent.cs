namespace Omnius.Axus.Interactors.Internal.Models;

public partial class CachedNoteContent
{
    public IEnumerable<CachedMemo> ToMemos()
    {
        var results = new List<CachedMemo>();

        foreach (var memo in this.Value.Memos)
        {
            var result = new CachedMemo(this.Signature, memo);
            results.Add(result);
        }

        return results;
    }
}
