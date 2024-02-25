﻿using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine("Specifica l'url del websocket (wss://...)");
            return 1;
        }
        Uri uri;
        try
        {
            uri = new(args[0]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"L'url del websocket non è valido: {ex.Message}");
            return 1;
        }
        using ClientWebSocket ws = new();
        ws.Options.RemoteCertificateValidationCallback = delegate (object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                Console.WriteLine("Nessun certificato");
            }
            else
            {
                Console.WriteLine($"Subject del certificato: {certificate.Subject}");
                string dnsNames = "";
                string ips = "";
                if (certificate is X509Certificate2 certificate2)
                {
                    foreach (var extension in certificate2.Extensions)
                    {
                        if (extension is X509SubjectAlternativeNameExtension san)
                        {
                            foreach (var dnsName in san.EnumerateDnsNames())
                            {
                                if (dnsNames.Length > 0)
                                {
                                    dnsNames += ", ";
                                }
                                dnsNames += dnsName;
                            }
                            foreach (var ip in san.EnumerateIPAddresses())
                            {
                                if (ips.Length > 0)
                                {
                                    ips += ", ";
                                }
                                ips += ip.ToString();
                            }
                        }
                    }
                    if (dnsNames.Length == 0)
                    {
                        dnsNames = "(nessuno)";
                    }
                    if (ips.Length == 0)
                    {
                        ips = "(nessuno)";
                    }
                    Console.WriteLine($"DSN alternative names: {dnsNames}");
                    Console.WriteLine($"IP alternative names: {ips}");
                }
            }
            return true;
        };
        try
        {
            await ws.ConnectAsync(uri, default);
            var bytes = new byte[1024];
            var result = await ws.ReceiveAsync(bytes, default);
            string res = Encoding.UTF8.GetString(bytes, 0, result.Count);
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);
            Console.WriteLine("Tutto ok");
            return 1;
        }
        catch (Exception ex)
        {
            Exception inn = ex;
            while (inn.InnerException != null)
            {
                inn = inn.InnerException;
            }
            Console.WriteLine($"{inn.Message}");
            return 1;
        }
    }
}