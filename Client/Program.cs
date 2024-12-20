using Client.Models;
using System.Diagnostics;
using System.Management;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

class Program
{

    static async Task Main()
    {
        try
        {
            // دریافت ورودی تعداد داده‌ها از کاربر
            Console.Write("Enter the number of data sets to send: ");
            int count = int.Parse(Console.ReadLine());


            // ارسال داده‌ها در پس‌زمینه
            var sendDataTask = Task.Run(() => SendData(count));


            Console.WriteLine("You can still use the application while sending data...");




            // منتظر پایان ارسال داده‌ها نباشید تا زمانی که کاربر برنامه را ببندد
            await sendDataTask;

            Console.WriteLine("Connection closed.");

            Console.ReadKey();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");

            Console.ReadKey();
        }

    }

    public static async Task SendData(int count)
    {
        // تولید و ارسال داده‌ها
        for (int i = 0; i < count; i++)
        {
            Console.WriteLine(new string('=', 30));

            // اتصال به سرور
            string serverIp = "127.0.0.1";
            int port = 5000;
            TcpClient client = new TcpClient(serverIp, port);
            Console.WriteLine("Connected to server.");

            NetworkStream stream = client.GetStream();

            // دریافت مصرف CPU و RAM از سیستم
            var cpuUsage = await GetCpuUsage();
            var ramUsage = await GetRamUsage();

            // تولید اعداد تصادفی
            int[] numbersList = new int[5];
            Random rand = new Random();
            for (int y = 0; y < 5; y++)
            {
                numbersList[y] = rand.Next(1, 2000); // اعداد تصادفی بین 1 تا 2000
            }

            // ایجاد داده‌ها
            ClientData myData = new ClientData
            {
                Id = rand.Next(1, 5000),
                CpuUsage = cpuUsage,
                RamUsage = ramUsage,
                NumbersList = numbersList
            };

            // تبدیل داده‌ها به JSON
            string json = JsonSerializer.Serialize(myData);

            try
            {
                // ارسال داده‌ها به سرور
                byte[] data = Encoding.UTF8.GetBytes(json);
                stream.Write(data, 0, data.Length);
                Console.WriteLine($"Sent JSON: {json}");

                // دریافت پاسخ از سرور
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // تبدیل پاسخ دریافتی از JSON به شیء
                string? responseData = JsonSerializer.Deserialize<string>(responseJson);
                Console.WriteLine("Response from server: " + responseData);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending/receiving data: {ex.Message}");
            }

            // بستن اتصال
            client.Close();
            Console.WriteLine(new string('=', 30));
        }
    }

    static async Task<float> GetCpuUsage()
    {
        // جستجو برای WMI اطلاعات پردازنده
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");

        foreach (ManagementObject queryObj in searcher.Get())
        {
            // استخراج درصد استفاده از CPU
            return Convert.ToSingle(queryObj["LoadPercentage"]);
        }

        return 0f;
    }

    // متد برای گرفتن درصد استفاده از RAM
    static async Task<float> GetRamUsage()
    {
        // جستجو برای WMI اطلاعات حافظه
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

        foreach (ManagementObject queryObj in searcher.Get())
        {
            // استخراج مقدار حافظه در دسترس و کل حافظه
            ulong totalMemory = (ulong)queryObj["TotalVisibleMemorySize"];
            ulong freeMemory = (ulong)queryObj["FreePhysicalMemory"];

            // محاسبه درصد استفاده از RAM
            float usedMemory = (float)(totalMemory - freeMemory);
            float ramUsagePercentage = (usedMemory / (float)totalMemory) * 100;

            return ramUsagePercentage;
        }

        return 0f;
    }


}
