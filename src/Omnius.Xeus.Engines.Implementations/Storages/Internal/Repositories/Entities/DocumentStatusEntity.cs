using System;
using System.Linq;
using LiteDB;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class DocumentStatusEntity
    {
        [BsonId]
        public string? Name { get; set; }

        public int Version { get; set; }
    }
}
