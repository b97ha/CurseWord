using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MovieProfanityDetector.Data;
using MovieProfanityDetector.Models;
using MovieProfanityDetector.Models.Base;
using MovieProfanityDetector.Services;

namespace MovieProfanityDetector
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //var appSettingsSection = Configuration.GetSection("ElasticSetting");
            //services.Configure<ElasticSetting>(appSettingsSection);
            //var elasticSetting = appSettingsSection.Get<ElasticSetting>();

            //var settings = new ConnectionSettings(new Uri(elasticSetting.BaseUrl));
            //services.AddSingleton<IElasticClient>(new ElasticClient(settings));


            services.AddControllers();


            services.AddCors(options =>
            {
                options
                    .AddPolicy("Cors", builder => builder
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .AllowAnyHeader());
            });

            services.Configure<AwsSettings>(Configuration.GetSection("AwsSettings"));

            services.AddRepository();
            services.AddSwaggerDocumentation();
            services.AddScoped(provider => new BaseResponse());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env ,ApplicationDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerDocumentation();
            }

            context.Database.Migrate();

            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot");

            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
            }

            app.UseStaticFiles(new StaticFileOptions()
            {

                FileProvider = new PhysicalFileProvider(wwwrootPath),
                RequestPath = new PathString(""),
               
            });

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("Cors");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
