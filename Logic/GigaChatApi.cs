using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators.OAuth2;
using Shavkat_grabber.Models;
using Shavkat_grabber.Models.Json;

namespace Shavkat_grabber.Logic;

public class GigaChatApi : IDisposable
{
    private const string ApiUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
    private const string MessageUrl =
        "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";

    private readonly HttpClient _httpClient;
    private readonly RestClient _restClient;
    private readonly string _scope;
    private readonly string _authKey;
    private OAuth2Token? _accessToken = null;

    public GigaChatApi(string authKey, string scope)
    {
        _authKey = authKey;
        _scope = scope;
        Console.WriteLine($"_authKey: {_authKey}, _scope: {_scope}");

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };

        _httpClient = new HttpClient(handler);
        _restClient = new RestClient(new HttpClient(handler));
    }

    private async Task<Result<OAuth2Token>> TryGetAccessToken()
    {
        var postData = new List<KeyValuePair<string, string>> { new("scope", _scope) };

        using var content = new FormUrlEncodedContent(postData);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        _httpClient.DefaultRequestHeaders.Add("RqUID", Guid.NewGuid().ToString());
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            _authKey
        );

        try
        {
            Console.WriteLine("Requesting access token...");
            var response = await _httpClient.PostAsync(ApiUrl, content);
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Token response: {json}");

            response.EnsureSuccessStatusCode();

            _accessToken = JsonConvert.DeserializeObject<OAuth2Token>(json);
            return Result<OAuth2Token>.Success(_accessToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting token: {ex}");
            return Result<OAuth2Token>.Fail(ex);
        }
    }

    public async Task<Result<AnswerRoot>> SendMessage(string message)
    {
        // Проверка и обновление токена
        if (_accessToken == null || DateTime.UtcNow.Ticks >= _accessToken.expires_at)
        {
            var tokenResult = await TryGetAccessToken();
            if (!tokenResult.IsSuccess)
                return Result<AnswerRoot>.Fail(tokenResult.Error);
        }

        var request = new RestRequest(MessageUrl, Method.Post)
            .AddHeader("Accept", "application/json")
            .AddHeader("Content-Type", "application/json")
            .AddHeader("RqUID", Guid.NewGuid().ToString())
            .AddJsonBody(
                new GigaChatMessageJson
                {
                    model = "GigaChat-2",
                    messages = new List<GigaChatMessage>
                    {
                        new() { content = message, role = "user" },
                    },
                }
            );

        request.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(
            _accessToken.access_token,
            "Bearer"
        );

        try
        {
            var response = await _restClient.ExecuteAsync(request);
            Console.WriteLine(
                $"Response status: {response.StatusCode}, content: {response.Content}"
            );

            if (!response.IsSuccessful)
            {
                return Result<AnswerRoot>.Fail(
                    response.ErrorException
                        ?? new Exception($"Request failed: {response.StatusCode}")
                );
            }

            return string.IsNullOrEmpty(response.Content)
                ? Result<AnswerRoot>.Fail(new Exception("Empty response"))
                : Result<AnswerRoot>.Success(
                    JsonConvert.DeserializeObject<AnswerRoot>(response.Content)
                );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex}");
            return Result<AnswerRoot>.Fail(ex);
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _restClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
