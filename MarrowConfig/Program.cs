namespace MarrowConfig;

using System.Runtime.InteropServices;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using CommandLine;

class Program
{
    public static ConsoleColor DefaultConsoleColor = Console.ForegroundColor;
    static readonly string LocalFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Marrow.conf";
    static Dictionary<string, string>? ConfigValues;
    static AdbClient? adbClient;
    static DeviceData deviceData;
    static Options? options;
    static string? Preset;
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(CreateOptionsObject);
        if (File.Exists(LocalFilePath))
        {
            File.Delete(LocalFilePath);
        }
        try
        {
            ConfigValues = SetupDefaultDictonaries(options.Perset);
        }
        catch (Exception)
        {
            Environment.Exit(1);
        }

        CreateFile(LocalFilePath, GetValues(ConfigValues));
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nPlease make sure you are connected to the Robot's WIFI network!\n");
        Console.ForegroundColor = DefaultConsoleColor;
        Console.WriteLine("Connecting to the Robot, please wait...\n");

        try
        {
            ConnectToAdbDevice(options.RobotIp, options.RobotPort);
            System.Console.WriteLine($"Connected to Robot at {options.RobotIp}:{options.RobotPort}\n");
            System.Console.WriteLine("Uploading Marrow.conf to the Robot's SDCard, please wait...\n");
            UploadMarrowConf(LocalFilePath);
            Console.WriteLine("Marrow.conf has been successfully uploaded to the Robot's SDCard at \"/sdcard/config/Marrow.conf\"\n");
            Console.WriteLine("Disconnecting from the Robot, please wait...\n"); 
            AdbDisconnect(options.RobotIp, options.RobotPort);
        }
        catch (AdbException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"An error occurred while trying to connect to the Robot: {e.Message}");
            Console.ForegroundColor = DefaultConsoleColor;
            Environment.Exit(1);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"An unexpected error occurred: {e.Message}");
            Console.ForegroundColor = DefaultConsoleColor;
            Environment.Exit(1);
        }
        File.Delete(LocalFilePath);
        Console.WriteLine("Disconnected from the Robot. The program will now exit.");
        Environment.Exit(0);}
    static void CreateOptionsObject(Options opts)
    {
        options = opts;
    }

    static Dictionary<string,string> GetValues(Dictionary<string, string> keyValuePairs)
    {
        string[] Keys = [.. keyValuePairs.Keys];
        if (Preset is null)
        {
            Preset = "";
        }

        string? input;
        for (int i = 0; i < keyValuePairs.Count; i++)
        {
            do
            {
                Console.Write($"Whats is the {SplitStringAt(Keys[i], Preset)}: ");
                input = Console.ReadLine();
                if (String.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Value can't be empty");
                }
            } while (String.IsNullOrEmpty(input));
            keyValuePairs[Keys[i]] = input;
        }
        return keyValuePairs;
    }

    static void CreateFile(string FilePath, Dictionary<string, string> keyValuePairs)
    {
        string[] KeyValue = new string[keyValuePairs.Count];
        string[] key = [.. keyValuePairs.Keys];
        string[] value = [.. keyValuePairs.Values];
        File.AppendAllLines(FilePath, ["#######", "##FTP##", "#######", " "]);
        for (int i = 0; i < keyValuePairs.Count; i++)
        {
            KeyValue[i] = $"{key[i]} = \"{value[i]}\"";
        }
        File.AppendAllLines(FilePath, KeyValue);
    }
    static string SplitStringAt(string str, string splitAfter)
    {
        int index = str.IndexOf(splitAfter);
        if (index == -1)
        {
            return str;
        }
        return str.Insert(index + splitAfter.Length, " ");
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
    public static Dictionary<string, string>? SetupDefaultDictonaries(char preset)
    {
        switch (preset)
        {
            case 'f':
                Preset = "Ftp";
                return new Dictionary<string, string>
                {
                    {"FtpServer", ""},
                    {"FtpPort", ""},
                    {"FtpUsername", ""},
                    {"FtpPassword", ""}
                };
            default:
                return null;
        }
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
    static void ConnectToAdbDevice(string Ip, string Port)
    {
        if (Ip is null || Port is null)
        {
            throw new ArgumentException("""
            Congrats! 
            You got to the part where the program needs to connect to the Robot. and still have at least 1 null argument, even after my many checks.
            I have nothing to say. You are just that special. 
            The program will now terminate, please retry to run it.
            Have a great rest of your day. 

            P.S If you see this message while using the program and aren't intentionally messing with it, send me a screenshot on discord @opi3636.
            """);
        }
        StartAdbServer();
        adbClient = new AdbClient();
        adbClient.Connect($"{Ip}:{Port}");
        deviceData = adbClient.GetDevices().FirstOrDefault();
    }

    static void AdbDisconnect(string Ip, string Port)
    {
        if (adbClient is null)
        {
            throw new AdbException("adbClient object has not been properly initialized");
        }
        adbClient.Disconnect($"{Ip}:{Port}");
    }
    static void UploadMarrowConf(string FilePath)
    {
        if (adbClient is null)
        {
            throw new AdbException("adbClient object has not been properly initialized");
        }
        RunShellCommand(adbClient, deviceData, "rm -f /sdcard/config/Marrow.conf");
        RunShellCommand(adbClient, deviceData, "mkdir /sdcard/config");

        using (SyncService syncService = new SyncService(deviceData))
        {
            using (FileStream fileStream = File.OpenRead(FilePath))
            {
                syncService.Push(fileStream, "/sdcard/config/Marrow.conf", UnixFileStatus.DefaultFileMode, DateTimeOffset.Now, null);
            }
        }     
    }
}
