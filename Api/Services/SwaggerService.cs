using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MovieProfanityDetector.Services
{
    public static class SwaggerService
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(swagger =>
            {
                swagger.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
                    // Exclude all DevExpress reporting controllers
                    return !methodInfo.DeclaringType.AssemblyQualifiedName.StartsWith("DevExpress",
                        StringComparison.OrdinalIgnoreCase);
                });

                swagger.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                //This is to generate the Default UI of Swagger Documentation  
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Baha Joher Web API Template",
                    Description = "Baha Joher Web API Template Built With ASP.NET 5",
                    Contact = new OpenApiContact
                    {
                        Name = "Baha Joher",
                        Email = string.Empty,
                        Url = new Uri("https://github.com/b97ha"),
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //swagger.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                swagger.SchemaFilter<AutoRestSchemaFilter>();
                // Use method name as operationId
                swagger.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });
                // To Enable authorization using Swagger (JWT)  
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
                // add Basic Authentication
                var basicSecurityScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    Reference = new OpenApiReference {Id = "BasicAuth", Type = ReferenceType.SecurityScheme}
                };
                swagger.AddSecurityDefinition(basicSecurityScheme.Reference.Id, basicSecurityScheme);
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {basicSecurityScheme, new string[] { }}
                });

                swagger.DescribeAllParametersInCamelCase();
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.DefaultModelsExpandDepth(-1);
                c.EnableValidator();
                c.EnableFilter();
                c.DocumentTitle = "APIs";
                c.DocExpansion(DocExpansion.None);
                c.ShowExtensions();
                c.DisplayRequestDuration();
                c.DisplayOperationId();
                c.EnableDeepLinking();
                c.ShowCommonExtensions();
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.DefaultModelExpandDepth(2);
                c.DefaultModelRendering(ModelRendering.Model);
            });

            return app;
        }
    }


    public class AutoRestSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;
            if (type.IsEnum)
            {
                schema.Extensions.Add(
                    "x-ms-enum",
                    new OpenApiObject
                    {
                        ["name"] = new OpenApiString(type.Name),
                        ["modelAsString"] = new OpenApiBoolean(true)
                    }
                );
            }

            ;
        }
    }
}