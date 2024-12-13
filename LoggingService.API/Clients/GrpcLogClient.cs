using Grpc.Net.Client;
using LoggingService.Grpc;

namespace LoggingService.API.Clients
{
    public class GrpcLogClient
    {
        private readonly LogService.LogServiceClient _client;

        public GrpcLogClient(string serverUrl)
        {
            var channel = GrpcChannel.ForAddress(serverUrl); // Ex: "https://localhost:5001"
            _client = new LogService.LogServiceClient(channel);
        }

        public async Task SendLogAsync(string serviceName, string logLevel, string message)
        {
            var request = new LogRequest
            {
                ServiceName = serviceName,
                LogLevel = logLevel,
                Message = message,
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            var response = await _client.SendLogAsync(request);
            Console.WriteLine($"Resposta do servidor: {response.Status}");
        }
    }
}