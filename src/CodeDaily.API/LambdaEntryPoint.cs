using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CodeDaily.API.Features.Auth;

namespace CodeDaily.API;

/// <summary>
/// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
/// actual Lambda function entry point. The Lambda handler field should be set to
/// 
/// CodeDaily.API::CodeDaily.API.LambdaEntryPoint::FunctionHandlerAsync
/// </summary>
public class LambdaEntryPoint :

    // The base class must be set to match the AWS service invoking the Lambda function. If not Amazon.Lambda.AspNetCoreServer
    // will fail to convert the incoming request correctly into a valid ASP.NET Core request.
    //
    // API Gateway REST API                         -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    // API Gateway HTTP API payload version 1.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    // API Gateway HTTP API payload version 2.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
    // Application Load Balancer                    -> Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
    // 
    // Note: When using the AWS::Serverless::Function resource with an event type of "HttpApi" then payload version 2.0
    // will be the default and you must make Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction the base class.

    Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
{
    /// <summary>
    /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
    /// needs to be configured in this method using the UseStartup<>() method.
    /// </summary>
    /// <param name="builder">The IWebHostBuilder to configure.</param>
    protected override void Init(IWebHostBuilder builder)
    {
        builder
            .ConfigureServices(services =>
            {
                // Configure JWT from SSM Parameter Store for Lambda
                var jwtConfig = GetJwtConfigurationFromSsm().GetAwaiter().GetResult();
                services.AddSingleton(jwtConfig);
            })
            .UseStartup<Startup>();
    }

    private async Task<JwtConfiguration> GetJwtConfigurationFromSsm()
    {
        using var ssmClient = new AmazonSimpleSystemsManagementClient();

        var request = new GetParametersRequest
        {
            Names = new List<string>
            {
                "/codedaily/jwt/private-key",
                "/codedaily/jwt/public-key",
                "/codedaily/jwt/expiration"
            },
            WithDecryption = true
        };

        var response = await ssmClient.GetParametersAsync(request);

        var privateKey = response.Parameters.FirstOrDefault(p => p.Name == "/codedaily/jwt/private-key")?.Value
            ?? throw new InvalidOperationException("JWT private-key not found in SSM Parameter Store");
        var publicKey = response.Parameters.FirstOrDefault(p => p.Name == "/codedaily/jwt/public-key")?.Value
            ?? throw new InvalidOperationException("JWT public-key not found in SSM Parameter Store");
        var expirationStr = response.Parameters.FirstOrDefault(p => p.Name == "/codedaily/jwt/expiration")?.Value
            ?? throw new InvalidOperationException("JWT expiration not found in SSM Parameter Store");

        if (!int.TryParse(expirationStr, out var expirationMinutes) || expirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT expiration must be a positive integer representing minutes.");
        }

        return new JwtConfiguration(privateKey, publicKey, expirationMinutes);
    }

    /// <summary>
    /// Use this override to customize the services registered with the IHostBuilder. 
    /// 
    /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
    /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
    /// </summary>
    /// <param name="builder">The IHostBuilder to configure.</param>
    protected override void Init(IHostBuilder builder)
    {
        // Empty - AWS services configured in WebHostBuilder Init method above
    }
}