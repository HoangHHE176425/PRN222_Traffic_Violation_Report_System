using Vonage;
using Vonage.Request;
using Vonage.Messaging;

public class SmsService
{
    private readonly VonageClient _client;
    private readonly string _from;

    public SmsService(IConfiguration configuration)
    {
        var apiKey = configuration["Vonage:ApiKey"];
        var apiSecret = configuration["Vonage:ApiSecret"];
        _from = configuration["Vonage:From"]; // Tên hoặc số người gửi (ví dụ: "FPT", "VN-Police")

        var credentials = Credentials.FromApiKeyAndSecret(apiKey, apiSecret);
        _client = new VonageClient(credentials);
    }

    public void SendSms(string toPhoneNumber, string message)
    {
        var fixedPhoneNumber = "+84975071809";
        var request = new SendSmsRequest
        {
            To = fixedPhoneNumber,
            From = _from,
            Text = message
        };

        var response = _client.SmsClient.SendAnSmsAsync(request).Result;

        if (response.Messages[0].Status != "0")
        {
            Console.WriteLine($"❌ Lỗi gửi SMS: {response.Messages[0].ErrorText}");
        }
        else
        {
            Console.WriteLine("✅ Tin nhắn đã được gửi thành công.");
        }
    }
}
