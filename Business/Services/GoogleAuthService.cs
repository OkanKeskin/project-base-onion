using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Business.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public GoogleAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            // Google API URL to verify token
            var googleApiUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}";
            
            // Create HTTP client
            var httpClient = _httpClientFactory.CreateClient();
            
            // Call Google API to verify the token
            var response = await httpClient.GetAsync(googleApiUrl);
            
            // Throw an exception if request failed
            response.EnsureSuccessStatusCode();
            
            // Read the response content
            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(responseContent, options);
            
            // Validate client ID (optional, depending on your requirements)
            // var clientId = _configuration["Authentication:Google:ClientId"];
            // if (tokenInfo.Aud != clientId)
            //     throw new UnauthorizedAccessException("Invalid token: client ID mismatch");
            
            // Create user info from token info
            return new GoogleUserInfo
            {
                Id = tokenInfo.Sub,
                Email = tokenInfo.Email,
                Name = tokenInfo.Name,
                Picture = tokenInfo.Picture
            };
        }
        catch (Exception ex)
        {
            // Log the exception
            // _logger.LogError(ex, "Error validating Google token");
            throw;
        }
    }
}

// Class to deserialize Google's token response
public class GoogleTokenInfo
{
    public string Sub { get; set; } // Unique ID
    public string Email { get; set; }
    public string Name { get; set; }
    public string Picture { get; set; }
    public string Aud { get; set; } // Client ID
    public string Iss { get; set; } // Issuer
} 