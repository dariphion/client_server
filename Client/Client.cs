using System;
using System.Net;
using System.Text;

public class Client
{
    public static void Main(string[] args)
    
    {
        while (true)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:8000");
            request.Method = "POST";

            byte[] data = Encoding.UTF8.GetBytes(Console.ReadLine());
            request.ContentType = "text/plain";
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            { }
        }
    }

}
