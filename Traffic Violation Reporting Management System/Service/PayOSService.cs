using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

public class PayOSResponse
{
    public string Code { get; set; } = "";
    public string Desc { get; set; } = ""; // sửa tên thuộc tính để phù hợp với JSON
    public PayOSData? Data { get; set; }
}

public class PayOSData
{
    public string? CheckoutUrl { get; set; }
}

public class PayOSService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PayOSService> _logger;
    private readonly string _clientId;
    private readonly string _apiKey;
    private readonly string _checksumKey;
    private readonly string _endpoint;
    private readonly string _baseUrl;

    public PayOSService(HttpClient httpClient, ILogger<PayOSService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _clientId = config["PayOS:ClientId"];
        _apiKey = config["PayOS:ApiKey"];
        _checksumKey = config["PayOS:ChecksumKey"];
        _endpoint = config["PayOS:Endpoint"];
        _baseUrl = config["PayOS:BaseUrl"];
    }

    public async Task<string> CreatePayment(
        int orderCode,
        int amount,
        string description,
        string returnUrl,
        string buyerPhone,
        string buyerName,
        string buyerEmail)
    {
        var cancelUrl = returnUrl;

        var rawSignature = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
        _logger.LogInformation("Chuỗi rawSignature: {rawSignature}", rawSignature);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawSignature));
        var signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

        var requestBody = new
        {
            orderCode,
            amount,
            description,
            returnUrl,
            cancelUrl,
            buyerName,
            buyerEmail,
            buyerPhone,
            signature
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonString = JsonSerializer.Serialize(requestBody, jsonOptions);

        _logger.LogInformation("Dữ liệu JSON gửi đi: {json}", jsonString);

        var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        request.Headers.Add("x-client-id", _clientId);
        request.Headers.Add("x-api-key", _apiKey);
        request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Phản hồi từ PayOS: {response}", responseContent);

        var payOSResponse = JsonSerializer.Deserialize<PayOSResponse>(responseContent, jsonOptions);

        if (payOSResponse == null || payOSResponse.Code != "00")
        {
            throw new Exception("Lỗi từ PayOS: " + responseContent);
        }

        return payOSResponse.Data?.CheckoutUrl ?? throw new Exception("Không tìm thấy checkoutUrl trong phản hồi.");
    }
}
