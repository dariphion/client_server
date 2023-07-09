using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Npgsql;

class Server
{
    public static HttpListener listener;
    public static string url = "http://localhost:8000/";

    public static async Task HandleIncomingConnections()
    {
        bool runServer = true;

        // While a user hasn't visited the `shutdown` url, keep on handling requests
        while (runServer)
        {
            // Will wait here until we hear from a connection
            HttpListenerContext ctx = await listener.GetContextAsync();

            // Peel out the requests and response objects
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;
            if (req.HttpMethod!="POST")
                {
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
                resp.Close();
                continue;
            }


            // Print out some info about the request
            Console.WriteLine(req.Url.ToString());
            Console.WriteLine(req.HttpMethod);
            using (StreamReader reader = new StreamReader(req.InputStream))
            {
                string streamtext = reader.ReadToEnd();
                Console.WriteLine($"The content is: {streamtext}");
                SaveToDB(streamtext);
            }
            resp.ContentType = "application/json";
            resp.ContentEncoding = Encoding.UTF8;

            // Write out to the response stream (asynchronously), then close it
            resp.Close();
        }
    }


    public static void Main(string[] args)
    {
        // Create a Http server and start listening for incoming connections
        listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();
        Console.WriteLine("Listening for connections on {0}", url);

        // Handle requests
        Task listenTask = HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        // Close the listener
        listener.Close();
    }

    public static void SaveToDB(string text)
    {
        string connectionString = "Host=localhost;Username=root;Password=root;Database=sample_db";
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            object result = null;
            using (NpgsqlCommand command = new NpgsqlCommand($"select MAX(ID) AS MaxID from Messages;", connection))
            {
                result = command.ExecuteScalar();
            }

            int highestID = (int)result;
            string sql = $"INSERT INTO Messages (ID, Content) VALUES ({(int)++highestID}, '{text}');";
            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }

            connection.Close();
        }

    }
}