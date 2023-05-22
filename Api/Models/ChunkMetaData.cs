
namespace MovieProfanityDetector.Models
{
    public class ChunkMetaData
    {
        public int Index { get; set; }
        public int TotalCount { get; set; }
        public long FileSize { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileGuid { get; set; }
    }
}
