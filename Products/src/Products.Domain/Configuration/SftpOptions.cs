namespace Products.Domain.Configuration;

public sealed class SftpOptions
{
    public const string SectionName = "Sftp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 2222;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string BasePath { get; set; } = "/upload";
}
