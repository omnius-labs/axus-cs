using LiteDB;
using Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Helpers
{
    internal static class VersionHelper
    {
        public static int GetVersion(ILiteDatabase database, string name)
        {
            var col = database.GetCollection<DocumentStatusEntity>("_document_status");

            var entry = col.FindOne(n => n.Name == name);
            if (entry is null)
            {
                return 0;
            }

            return entry.Version;
        }

        public static void SetVersion(ILiteDatabase database, string name, int Version)
        {
            var col = database.GetCollection<DocumentStatusEntity>("_document_status");

            col.Upsert(new DocumentStatusEntity() { Name = name, Version = Version });
        }
    }
}
