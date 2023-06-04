using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    private TcpListener listener;
    private List<ClientHandler> clients;
    private Dictionary<string, string> userCredentials;
    private Dictionary<IPAddress, int> failedAttempts;
    private int processedCommands;

    private string logFilePath = "server_log.txt";
    private string credentialsFilePath = "credentials.txt";

    public Server()
    {
        listener = new TcpListener(IPAddress.Any, 8888);
        clients = new List<ClientHandler>();
        userCredentials = new Dictionary<string, string>();
        failedAttempts = new Dictionary<IPAddress, int>();
        processedCommands = 0;
    }

    public void Start()
    {
        LoadCredentials();

        listener.Start();
        Console.WriteLine("Server started. Waiting for connections...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            ClientHandler clientHandler = new ClientHandler(client, this);
            clients.Add(clientHandler);
            clientHandler.Start();
        }
    }

    public void HandleCommand(ClientHandler clientHandler, string command)
    {
        string[] parts = command.Split(' ');
        string response = "";

        switch (parts[0])
        {
            case "login":
                if (parts.Length == 3)
                {
                    string username = parts[1];
                    string password = parts[2];

                    if (Login(username, password))
                    {
                        clientHandler.Username = username;
                        response = "Logged in successfully.";
                        LogEvent(username, clientHandler.Client.Client.RemoteEndPoint.ToString());
                    }
                    else
                    {
                        response = "Invalid username or password.";

                        IPAddress clientAddress = ((IPEndPoint)clientHandler.Client.Client.RemoteEndPoint).Address;
                        if (!failedAttempts.ContainsKey(clientAddress))
                            failedAttempts[clientAddress] = 0;
                        failedAttempts[clientAddress]++;

                        if (failedAttempts[clientAddress] >= 3)
                        {
                            BlockIP(clientAddress);
                            response += " Your IP address has been blocked.";
                        }
                    }
                }
                else
                {
                    response = "Invalid login command.";
                }
                break;

            case "who":
                response = "Currently logged in users:\n";
                foreach (ClientHandler client in clients)
                {
                    if (!string.IsNullOrEmpty(client.Username))
                        response += client.Username + "\n";
                }
                break;

            case "uptime":
                TimeSpan uptime = DateTime.Now - Program.StartTime;
                response = "Server uptime: " + uptime.ToString(@"dd\.hh\:mm\:ss");
                break;

            case "stats":
                response = "Statistics:\n";
                response += "Logged in users: " + clients.FindAll(c => !string.IsNullOrEmpty(c.Username)).Count + "\n";
                response += "Failed login attempts: " + failedAttempts.Count + "\n";
                response += "Processed commands: " + processedCommands + "\n";
                break;

            case "exit":
                response = "Goodbye!";
                clientHandler.Stop();
                clients.Remove(clientHandler);
                break;

            default:
                response = "Unknown command.";
                break;
        }

        clientHandler.SendResponse(response);
        processedCommands++;
    }

    private bool Login(string username, string password)
    {
        if (userCredentials.ContainsKey(username))
        {
            if (userCredentials[username] == password)
                return true;
        }

        return false;
    }

    private void BlockIP(IPAddress ipAddress)
    {
        // Implement IP blocking logic here
        Console.WriteLine("Blocked IP address: " + ipAddress);
    }

    private void LogEvent(string username, string clientAddress)
    {
        string logEntry = DateTime.Now.ToString() + " - " + username + " logged in from " + clientAddress + Environment.NewLine;
        File.AppendAllText(logFilePath, logEntry);
    }

    private void LoadCredentials()
    {
        if (File.Exists(credentialsFilePath))
        {
            string[] lines = File.ReadAllLines(credentialsFilePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    string username = parts[0];
                    string password = parts[1];
                    userCredentials[username] = password;
                }
            }
        }
        else
        {
            Console.WriteLine("Credentials file not found. Please create a file named 'credentials.txt' with username:password entries.");
            Environment.Exit(1);
        }
    }
}

class ClientHandler
{
    private TcpClient client;
    private Server server;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    public string Username { get; set; }

    public TcpClient Client { get { return client; } }

    public ClientHandler(TcpClient client, Server server)
    {
        this.client = client;
        this.server = server;
    }

    public void Start()
    {
        stream = client.GetStream();
        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);

        writer.WriteLine("Welcome to the server. Please login.");
        writer.Flush();

        while (true)
        {
            string command = reader.ReadLine();
            if (command != null)
                server.HandleCommand(this, command);
        }
    }

    public void SendResponse(string response)
    {
        writer.WriteLine(response);
        writer.Flush();
    }

    public void Stop()
    {
        writer.Close();
        reader.Close();
        stream.Close();
        client.Close();
    }
}

class Program
{
    public static DateTime StartTime { get; private set; }

    static void Main(string[] args)
    {
        Server server = new Server();
        StartTime = DateTime.Now;
        server.Start();
    }
}

