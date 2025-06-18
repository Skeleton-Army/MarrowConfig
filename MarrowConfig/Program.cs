namespace MarrowConfig;

using System.Runtime.InteropServices;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;

class Program
{
    public static ConsoleColor DefaultConsoleColor = Console.ForegroundColor;
    static string LocalFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Marrow.conf"; 
    readonly static string[] ConfigValues = ["FtpIp", "FtpPort", "FtpUsername", "FtpPassword"];
    static void Main(string[] args)
    {
        if (File.Exists(LocalFilePath))
        {
            File.Delete(LocalFilePath);
        }
        CreateFile(LocalFilePath, ConfigValues, GetValues(["Ftp server Ip addres", "Port for the Ftp connection (usally 20 or 21)", "username for the Ftp server", "password for the Ftp server"]));
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nPlease make sure you are connected to the Robot's WIFI network!");
        Console.ForegroundColor = DefaultConsoleColor;
        string[] AdbCofiguration = GetValues(["Robot's Ip", "Robot's adb port"]);
        ConnectAndUpload(AdbCofiguration[0], AdbCofiguration[1], LocalFilePath);
        File.Delete(LocalFilePath);
        Console.WriteLine("The config file has been successfully uploaded to the Robots SDCard at \"/sdcard/config/Marrow.conf\"");
    }
    static string[] GetValues(string[] field)
    {
    #pragma warning disable CS8601 // Possible null reference assignment.
        string[] Values = new string[field.Length];
        
        for (int i = 0; i < field.Length; i++)
        {
            Console.Write($"What is the {field[i]} : ");
            Values[i] = Console.ReadLine();
            while (String.IsNullOrEmpty(Values[i]))
            {
              Console.WriteLine("Value can't be empty!");
              Console.Write($"What is the {field[i]} : ");
              Values[i] = Console.ReadLine();

            }
        }
        return Values;
    #pragma warning restore CS8601 // Possible null reference assignment.
    }
    static void CreateFile(string FilePath, string[] key, string[] value)
    {
        string[] KeyValue = new string[key.Length];
        File.AppendAllLines(FilePath, ["#######", "##FTP##", "#######", " "]);
        for (int i = 0; i < key.Length; i++)
        {
            KeyValue[i] = $"{key[i]} = \"{value[i]}\"";
        }
        File.AppendAllLines(FilePath, KeyValue);
    }
    static string GetAdbLocation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Android\Sdk\platform-tools\adb.exe";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/Android/Sdk/platform-tools/adb";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/Library/Android/sdk/platform-tools/adb";
        }
        return "";
    }
    static void StartAdbServer()
    {
        if (!AdbServer.Instance.GetStatus().IsRunning)
        {
            AdbServer server = new AdbServer();
            StartServerResult result = server.StartServer(GetAdbLocation(), false);

            if (result != StartServerResult.Started)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Adb server Failed to start!");
                Console.ForegroundColor = DefaultConsoleColor;
                Environment.Exit(1);
            }
        }
    }
    static void RunShellCommand(AdbClient adbClient,DeviceData device, string command)
    {
        ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
        adbClient.ExecuteRemoteCommand(command, device, receiver);
    }
    static void ConnectAndUpload(string Ip, string Port, string FilePath)
    {
        StartAdbServer();
        AdbClient adbClient = new AdbClient();
        adbClient.Connect($"{Ip}:{Port}");
        DeviceData device = adbClient.GetDevices().FirstOrDefault();

        RunShellCommand(adbClient, device, "rm -f /sdcard/config/Marrow.conf");
        RunShellCommand(adbClient, device, "mkdir /sdcard/config");

        using (SyncService syncService = new SyncService(device))
        {
            using (FileStream fileStream = File.OpenRead(FilePath))
            {
                syncService.Push(fileStream, "/sdcard/config/Marrow.conf", UnixFileStatus.DefaultFileMode, DateTimeOffset.Now, null);
            }
        }     
        adbClient.Disconnect($"{Ip}:{Port}");
    }
}
