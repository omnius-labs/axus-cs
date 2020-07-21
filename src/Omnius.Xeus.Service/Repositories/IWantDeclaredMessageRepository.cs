public interface IWantDeclaredMessageRepository
{
    FetchAllWantSignatures();
    FetchWantSignature();
    AddWantSignature();
    RemoveWantSignature();

    FetchDeclaredMessage();
    FetchDeclaredMessageCreationTime();
    AddDeclaredMessage();
    RemoveDeclaredMessage();
}
