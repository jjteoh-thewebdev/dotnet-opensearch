

using OpenSearch.Client;
using OpenSearch.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var configuration = builder.Configuration;

// Configure OpenSearch client
builder.Services.AddSingleton<IOpenSearchClient>(sp =>
{
    // read credentials from appsettings
    var openSearchUrl = configuration.GetValue<string?>("OpenSearch:HostUrl") ?? "https://18.141.197.9:9200";
    var username = configuration.GetValue<string?>("OpenSearch:UserName") ?? "admin";
    var password = configuration.GetValue<string?>("OpenSearch:Password") ?? "Abc123";

    var settings = new ConnectionSettings(new Uri(openSearchUrl))
    .BasicAuthentication(username, password)
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
    return new OpenSearchClient(settings);
});


var app = builder.Build();
app.Urls.Add("http://localhost:5000");

app.MapControllers();
app.MapGet("/", () => "Hello World!");


app.Run();




