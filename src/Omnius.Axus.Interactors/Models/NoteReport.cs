using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Models;

public record NoteReport
{
    public NoteReport(OmniSignature signature, OmniHash selfHash, Note note)
    {
        this.Signature = signature;
        this.SelfHash = selfHash;
        this.Note = note;
    }

    public OmniSignature Signature { get; }
    public OmniHash SelfHash { get; }
    public Note Note { get; }
}
