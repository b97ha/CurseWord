using System;
using Microsoft.AspNetCore.Http;

namespace MovieProfanityDetector.Models.Dtos
{
    public class BookSave
    {
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string Description { get; set; }
        public DateTime? PublishDate { get; set; }
        public IFormFile CoverImage { get; set; }
        public IFormFile BookFile { get; set; }
    }

}
