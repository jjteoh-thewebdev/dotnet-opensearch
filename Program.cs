

using OpenSearch.Client;
using OpenSearch.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Configure OpenSearch client
builder.Services.AddSingleton<IOpenSearchClient>(sp =>
{
    var settings = new ConnectionSettings(new Uri("https://18.141.197.9:9200"))
    .BasicAuthentication("admin", "aRT/[sqA7^")
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
    return new OpenSearchClient(settings);
});


var app = builder.Build();
app.Urls.Add("http://localhost:5000");

app.MapControllers();
app.MapGet("/", () => "Hello World!");


app.Run();




