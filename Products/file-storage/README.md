# SFTP Integration Guide

This guide explains how to connect and upload files to an SFTP server hosted via Docker Compose from a .NET API.

## Prerequisites
- A running Docker Compose environment with the `atmoz/sftp` container.
- The `SSH.NET` NuGet package installed in your .NET project (e.g., `dotnet add package SSH.NET`).

## Docker Compose Setup

The SFTP server is defined in your `docker-compose.yaml` as follows:

```yaml
  sftp:
    image: atmoz/sftp:alpine
    container_name: catalog_sftp
    restart: always
    ports:
      - "2222:22"
    # user:pass:uid
    command: "FtpProductCatalogManager:xv43%17FWq@:1001"
    volumes:
      - ./catalog_images:/home/uploaduser/upload
```

This configuration exposes the SFTP server on `localhost:2222` with the credentials:
- **Username**: `FtpProductCatalogManager`
- **Password**: `xv43%17FWq@`
- **Upload path**: Files must be uploaded to the `/upload` directory.

The `/upload` directory inside the container is mapped to the `./catalog_images` volume on your local machine.

## .NET API Integration

To upload a file using the `.NET` API, use the `SftpClient` from the `SSH.NET` library.

### 1. Connection Details

Use the credentials and port from your Docker Compose configuration:

```csharp
private const string Host = "localhost";
private const int Port = 2222; 
private const string Username = "FtpProductCatalogManager";
private const string Password = "xv43%17FWq@";
```

### 2. Uploading a File

Here is an example of connecting to the SFTP server and uploading a file:

```csharp
using Renci.SshNet;
using System;
using System.IO;
using System.Threading.Tasks;

public async Task UploadFileAsync(string localFilePath)
{
    // Define the remote destination filename inside the SFTP upload folder
    string remoteFileName = $"product_{Guid.NewGuid():N}.avif";
    
    // IMPORTANT: The path must start with the directory mounted in Docker
    string remoteFilePath = $"/upload/{remoteFileName}";

    Console.WriteLine($"Connecting to SFTP server at {Host}:{Port}...");

    try
    {
        using (var client = new SftpClient(Host, Port, Username, Password))
        {
            // Connect to the SFTP server
            client.Connect();
            Console.WriteLine("Connected successfully!");

            // Open the local file stream
            await using (var fileStream = File.OpenRead(localFilePath))
            {
                Console.WriteLine($"Uploading '{localFilePath}' to '{remoteFilePath}'...");

                // Execute the upload
                await Task.Run(() => client.UploadFile(fileStream, remoteFilePath));
            }

            // Disconnect once the upload is complete
            client.Disconnect();
            Console.WriteLine("Upload completed and connection closed.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[ERROR] Upload failed: {ex.Message}");
    }
}
```

### 3. Accessing the Uploaded File

Once uploaded, the files are made available via NGINX and Imgproxy as configured in the Docker Compose stack. You can access the uploaded images at the following URLs (replacing `{remoteFileName}` with the actual file name):

- **Raw Original URL**: `http://localhost:3712/raw/{remoteFileName}`
- **Resized image (via Imgproxy)**: `http://localhost:3712/media/insecure/rs:fill:300:300/plain/local:///{remoteFileName}`
