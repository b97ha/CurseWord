using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieProfanityDetector.Models.Base;

namespace MovieProfanityDetector.Models.Entities
{
    public class Movie : BaseEntity
    {
        public string MovieName { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public bool IsTranscription  { get; set; }
        public string TransJobName { get; set; }
        public string FileName { get; set; }
    }

    public class BookConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.ToTable("Movies").HasKey(k => k.Guid);

            builder.Property(p => p.MovieName).IsRequired().HasMaxLength(250);

        }
    }
}
