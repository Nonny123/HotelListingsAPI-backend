using HotelListing.Configurations;
using HotelListing.Data;
using HotelListing.IRepository;
using HotelListing.Repository;
using HotelListing.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing
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

            //load db configuration
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("sqlConnection")));

            services.ConfigureHttpCacheHeaders();//implemented in ServiceExtension.cs
            services.AddAuthentication();
            services.ConfigureIdentity(); //implemented in ServiceExtension.cs
            services.ConfigureJWT(Configuration); //implemented in ServiceExtension.cs


            //Configure CORS
            services.AddCors(o => {
                o.AddPolicy("AllowAll", builder =>
                     builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });


            //Configure Automapper
            services.AddAutoMapper(typeof(MapperInitializer));


            //configure dependencies
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthManager, AuthManager>();


            //Configure Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HotelListing", Version = "v1" });
            });



            // install nuget - microsoft.aspnetcore.mvc.newtonsoftjson 
            services.AddControllers(config => {
                config.CacheProfiles.Add("120SecondsDuration", new CacheProfile 
                { 
                    Duration = 120
              
                });
            
            
            }).AddNewtonsoftJson(op =>
                op.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );


            services.ConfigureVersioning(); //implemented in ServiceExtension.cs

        }

       
        
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();            
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HotelListing v1"));

            
            app.ConfigureExceptionHandler();//implemented in ServiceExtension.cs

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");


            app.UseResponseCaching();
            app.UseHttpCacheHeaders();
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
