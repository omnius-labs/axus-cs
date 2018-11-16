using System.Runtime.Serialization;

namespace Amoeba.Messages
{
    public enum DownloadState
    {
        Downloading = 0,
        ParityDecoding = 1,
        Decoding = 2,
        Completed = 3,
        Error = 4,
    }
}
