using GPQ.Client.Pages;
using GPQ.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapPost("/api/proxy", async (GPQ.Client.Models.ProxyRequest request, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    client.Timeout = TimeSpan.FromSeconds(30);

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var responseModel = new GPQ.Client.Models.ProxyResponse();

    try
    {
        var url = request.Url;
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "http://" + url;
        }

        var method = new HttpMethod(request.Method.ToUpperInvariant());
        using var requestMessage = new HttpRequestMessage(method, url);

        foreach (var header in request.Headers)
        {
            if (string.IsNullOrWhiteSpace(header.Key) || !header.IsEnabled) continue;

            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            catch { }
        }

        if (method != HttpMethod.Get && method != HttpMethod.Head)
        {
            if (request.BodyMode == "form-data")
            {
                var multipartContent = new MultipartFormDataContent();
                foreach (var param in request.FormData)
                {
                    if (!param.IsEnabled || string.IsNullOrWhiteSpace(param.Key)) continue;
                    multipartContent.Add(new StringContent(param.Value ?? string.Empty), param.Key);
                }
                requestMessage.Content = multipartContent;
            }
            else if (request.BodyMode == "urlencoded")
            {
                var nvps = new List<KeyValuePair<string, string>>();
                foreach (var param in request.UrlEncodedData)
                {
                    if (!param.IsEnabled || string.IsNullOrWhiteSpace(param.Key)) continue;
                    nvps.Add(new KeyValuePair<string, string>(param.Key, param.Value ?? string.Empty));
                }
                requestMessage.Content = new FormUrlEncodedContent(nvps);
            }
        }

        using var httpResponse = await client.SendAsync(requestMessage);
        stopwatch.Stop();

        responseModel.StatusCode = (int)httpResponse.StatusCode;
        responseModel.StatusDescription = httpResponse.ReasonPhrase ?? httpResponse.StatusCode.ToString();
        responseModel.ElapsedTimeMs = stopwatch.ElapsedMilliseconds;

        foreach (var header in httpResponse.Headers)
        {
            responseModel.Headers.Add(new GPQ.Client.Models.KeyValuePairModel
            {
                Key = header.Key,
                Value = string.Join(", ", header.Value),
                IsEnabled = true
            });
        }
        if (httpResponse.Content != null)
        {
            foreach (var header in httpResponse.Content.Headers)
            {
                responseModel.Headers.Add(new GPQ.Client.Models.KeyValuePairModel
                {
                    Key = header.Key,
                    Value = string.Join(", ", header.Value),
                    IsEnabled = true
                });
            }

            var bodyBytes = await httpResponse.Content.ReadAsByteArrayAsync();
            responseModel.SizeInBytes = bodyBytes.Length;
            responseModel.Body = System.Text.Encoding.UTF8.GetString(bodyBytes);
        }
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        responseModel.StatusCode = 0;
        responseModel.StatusDescription = "Error";
        responseModel.ElapsedTimeMs = stopwatch.ElapsedMilliseconds;
        responseModel.ErrorMessage = ex.GetBaseException().Message;
    }

    return Results.Ok(responseModel);
});

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(GPQ.Client._Imports).Assembly);

app.Run();
