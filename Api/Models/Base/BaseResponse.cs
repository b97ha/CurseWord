using Microsoft.AspNetCore.Http;

namespace MovieProfanityDetector.Models.Base
{
    public class BaseResponse
    {
        public int Code { get; set; } = StatusCodes.Status200OK;
        public string Message { get; set; }
        public object Result { get; set; }
    }
}