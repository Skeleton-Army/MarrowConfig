using CommandLine;

namespace MarrowConfig;

public class Options
{
    [Option('p', "preset", Required = true, HelpText = "Marrow Config perset")]
    public char Perset { get; set; }

    [Option("adb_ip", Required = true, HelpText = "Ip of the Robot")]
    public string? RobotIp { get; set; }

    [Option("adb_port", Required = false, Default = "5555", HelpText = "Adb port of the Robot")]
    public string? RobotPort { get; set; }
}
