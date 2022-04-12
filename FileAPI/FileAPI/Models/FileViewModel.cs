using Microsoft.AspNetCore.Http;

namespace FileAPI.Models
{
    public class FileViewModel
    {
        public IFormFile MyFile { get; set; }
        public string Name { get; set; }
        public string AltText { get; set; }
        public string Description { get; set; }

    }
}
