namespace FileAPI.Models
{
    public partial class FileData
    {
        public int Id { get; set; }
        public string UniqueName { get; set; }
        public string Name { get; set; }
        public string FileExtension { get; set; }
        public string MimeType { get; set; }
        public string FilePath { get; set; }
        public string AltText { get; set; }
        public string Description { get; set; }

    }
}
