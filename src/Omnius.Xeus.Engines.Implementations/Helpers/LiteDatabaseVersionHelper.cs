using LiteDB;

namespace Omnius.Xeus.Engines.Helpers
{
    internal static class LiteDatabaseVersionHelper
    {
        public static int GetVersion(ILiteDatabase database, string name)
        {
            var col = database.GetCollection<DocumentStatusEntity>("_document_status");

            var entry = col.FindOne(n => n.Name == name);
            if (entry is null) return 0;

            return entry.Version;
        }

        public static void SetVersion(ILiteDatabase database, string name, int version)
        {
            var col = database.GetCollection<DocumentStatusEntity>("_document_status");

            col.Upsert(new DocumentStatusEntity() { Name = name, Version = version });
        }

        private sealed class DocumentStatusEntity
        {
            [BsonId]
            public string? Name { get; set; }

            public int Version { get; set; }
        }
    }
}
