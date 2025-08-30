using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;

namespace CodeDaily.API;

/// <summary>
/// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
/// </summary>
public class LocalEntryPoint
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen();
                
                services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());
                services.AddAWSService<IAmazonDynamoDB>();
                services.AddTransient<IDynamoDBContext, DynamoDBContext>();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}