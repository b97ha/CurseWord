using Microsoft.Extensions.DependencyInjection;
using MovieProfanityDetector.Data.IRepository;
using MovieProfanityDetector.Data.Repository;
using MovieProfanityDetector.Data.UnitOfWork;
using MovieProfanityDetector.Helpers;

namespace MovieProfanityDetector.Services
{
    public static class RepositoryService
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddTransient<IMoviesRepository, MoviesRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IUploader, Uploader>();


            services.AddTransient<IFFmpeg, FFmpegHelper>();
            services.AddTransient<IAwsSpeech, AwsSpeech>();

            return services;
        }
    }
}