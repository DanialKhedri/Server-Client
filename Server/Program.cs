using Microsoft.Extensions.DependencyInjection;
using Server.Data;
using Server.Entities;
using Server.Repository;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;

class Program
{
    // ایجاد SemaphoreSlim برای کنترل تعداد درخواست‌های همزمان
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    static async Task Main()
    {
        #region Configuration

        // Configure services
        var serviceProvider = new ServiceCollection()
            .AddDbContext<DataContext>(options =>
                options.UseSqlServer("Data Source=.;Initial Catalog=ServerDatabase;Integrated Security=True;Trust Server Certificate=True"))
            .AddScoped<IDataRepository, DataRepository>()
            .BuildServiceProvider();

        var _dataRepository = serviceProvider.GetService<IDataRepository>();

        #endregion


        #region Start TCP Listener

        // تعیین IP و پورت
        string ipAddress = "127.0.0.1";
        int port = 5000;

        // ایجاد TcpListener
        TcpListener server = new TcpListener(IPAddress.Parse(ipAddress), port);
        server.Start();
        Console.WriteLine($"Server started on {ipAddress}:{port}");

        #endregion

        while (true)
        {
            // منتظر اتصال کلاینت
            TcpClient client = await server.AcceptTcpClientAsync();
            Console.WriteLine("Client connected.");

            // هر درخواست را در یک Task جداگانه پردازش می‌کنیم
            _ = HandleClientAsync(client, _dataRepository);
        }
    }

    // این متد درخواست‌ها را به صورت غیر همزمان پردازش می‌کند
    private static async Task HandleClientAsync(TcpClient client, Server.Repository.IDataRepository _dataRepository)
    {
        // ابتدا SemaphoreSlim را وارد می‌کنیم تا از دسترسی همزمان جلوگیری کنیم
        await _semaphore.WaitAsync();

        try
        {
            // پردازش داده‌ها
            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine(new string('=', 30));

                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received JSON: {message}");

                // تبدیل JSON به شیء C#
                ClientData receivedData = JsonSerializer.Deserialize<ClientData>(message);
                Console.WriteLine($"Parsed Data: Id = {receivedData.Id}, CPU Usage = {receivedData.CpuUsage}, RAM Usage = {receivedData.RamUsage}");

                DataModel dataModel = new DataModel()
                {
                    ClientId = receivedData.Id,
                    CpuUsage = receivedData.CpuUsage,
                    RamUsage = receivedData.RamUsage,
                    Sum = receivedData.NumbersList.AsParallel().Where(n => n < 1000).Sum(),
                };

                bool result = await _dataRepository.AddDataToDataBase(dataModel);

                string responseMessage = result ? "Data Recived and Saved In Data Base" : "Error";

                // پاسخ به کلاینت
                string responseJson = JsonSerializer.Serialize(responseMessage);
                byte[] responseDataBytes = Encoding.UTF8.GetBytes(responseJson);
                await stream.WriteAsync(responseDataBytes, 0, responseDataBytes.Length);

                Console.WriteLine($"Sent JSON: {responseJson}");

                Console.WriteLine(new string('=', 30));
            }
        }
        finally
        {
            // پس از پردازش درخواست، SemaphoreSlim را آزاد می‌کنیم
            _semaphore.Release();
        }
    }

    public class ClientData
    {
        public int Id { get; set; }
        public float CpuUsage { get; set; }
        public float RamUsage { get; set; }
        public int[] NumbersList { get; set; }
    }
}

