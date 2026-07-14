using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

class Program {
    static void Main() {
        var client = new SecretClient(new Uri("https://kverpbqmn7izy.vault.azure.net/"), new DefaultAzureCredential());
        var secret = client.GetSecret("AzureBlobStorage--ConnectionString");
        Console.WriteLine(secret.Value.Value.Substring(0, 20) + "...");
    }
}
