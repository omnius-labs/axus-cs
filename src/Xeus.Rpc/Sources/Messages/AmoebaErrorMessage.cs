using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Amoeba.Rpc
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class AmoebaErrorMessage
    {
        [JsonConstructor]
        public AmoebaErrorMessage(string type, string message, string stackTrace)
        {
            this.Type = type;
            this.Message = message;
            this.StackTrace = stackTrace;
        }

        [JsonProperty]
        public string Type { get; private set; }

        [JsonProperty]
        public string Message { get; private set; }

        [JsonProperty]
        public string StackTrace { get; private set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(this.Type)}: {this.Type}");
            sb.AppendLine($"{nameof(this.Message)}: {this.Message}");
            sb.AppendLine($"{nameof(this.StackTrace)}: {this.StackTrace}");

            return sb.ToString();
        }
    }
}
