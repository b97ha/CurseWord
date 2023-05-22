using System;

namespace MovieProfanityDetector.Models.Base
{
    public class BaseEntity
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
    }
}
