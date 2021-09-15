using LiteDB;

namespace Omnius.Xeus.Utils
{
    public static class LiteDatabaseExtentions
    {
        public static int GetDocumentVersion(this ILiteDatabase database, string name)
        {
            var col = database.GetCollection<DocumentStatusEntity>("_document_status");

            var entry = col.FindById(name);
            if (entry is null) return 0;

            return entry.Version;
        }

        public static void SetDocumentVersion(this ILiteDatabase database, string name, int version)
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
