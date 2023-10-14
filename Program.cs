
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yarp.Configs;
using Yarp.ReverseProxy.Swagger;
using Yarp.ReverseProxy.Swagger.Extensions;

namespace YarpGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            builder.Services.AddSwaggerGen();

            builder.Services
                .AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("Yarp"))
                .AddSwagger(builder.Configuration.GetSection("Yarp")); ;

            //builder.Services.AddControllers();   

            var app = builder.Build();
            
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var config = app.Services.GetRequiredService<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>().CurrentValue;
                foreach (var cluster in config.Clusters)
                {
                    options.SwaggerEndpoint($"/swagger/{cluster.Key}/swagger.json", cluster.Key);
                }
            });

            app.UseHttpsRedirection();
            app.MapReverseProxy();            
            //app.UseAuthorization();
            //app.MapControllers();            
            //app.UseRouting();
            app.Run();
        }
    }
}