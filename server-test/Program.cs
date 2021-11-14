using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace servertest
{
    class MainClass
    {
        public static string res {get; set;}

        public static void Main(string[] args)
        {
            string port = Console.ReadLine();

            // URI prefixes are required,
            var prefixes = new List<string>() { "http://127.0.0.1:" + port + "/" };

            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
                Console.WriteLine(s);
            }
            listener.Start();
            Console.WriteLine("Listening...");
            while (true)
            {
                if (!listener.IsListening) { return; }
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = listener.GetContext();

                HandleContext(context, listener);

                
            }
            //listener.Stop();
        }

        private static void HandleContext(HttpListenerContext context, HttpListener listener)
        {
            HttpListenerRequest request = context.Request;

            //string documentContents;
            //using (Stream receiveStream = request.InputStream)
            //{
            //    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
            //    {
            //        documentContents = readStream.ReadToEnd();
            //    }
            //}

            // Obtain a response object.
            HttpListenerResponse response = context.Response;

            string method = request.Url.AbsolutePath;
            Console.WriteLine($"Recived request for {request.Url.AbsolutePath}");
            //Console.WriteLine(documentContents);


            DataSerializer dataSerializer = new DataSerializer("json");
            string serializedInput = @"{""K"":10,""Sums"":[1.01,2.02],""Muls"":[1,4]}";
            Input input = dataSerializer.Deserialized(serializedInput);
            Output output = dataSerializer.GetAnswer(input);

            response.StatusCode = (int)HttpStatusCode.OK;

            string responseString = "error";
            byte[] responseBody;
            switch (method)
            {
                case "/Ping":
                    responseString = $"{HttpStatusCode.OK}";
                    break;
                case "/GetInputData":

                    responseString = serializedInput;//,\"Output\":{\"SumResult\":30.30,\"MulResult\":4,\"SortedInputs\":[1.0,1.01,2.02,4.0]}}";
            
                    break;
                case "/WriteAnswer":

                    string body = request.QueryString["answer"];
                    body = body.Trim('"');
                   // Input desBody = dataSerializer.Deserialized(body);
                    //using (var reader = new StreamReader(request.InputStream,
                    //                 request.ContentEncoding))
                    //{
                    //    body = reader.ReadToEnd();
                    //}

                    string text = @"{""answer"": false}";

                    Console.WriteLine(body);
                    Console.WriteLine(dataSerializer.Serialized(output));

                    if (body == dataSerializer.Serialized(output))
                    {
                        text = @"{""answer"": true}";
                    }
                    responseString = text;//,\"Output\":{\"SumResult\":30.30,\"MulResult\":4,\"SortedInputs\":[1.0,1.01,2.02,4.0]}}";
                    


                    break;
                case "/PostInputData":
                    using (var reader = new StreamReader(request.InputStream,
                                     request.ContentEncoding))
                    {
                        res = reader.ReadToEnd();
                    }
                    break;
                case "/GetAnswer":

                    if (res == null)
                    {
                        break;
                    }

                    Input reqInput = dataSerializer.Deserialized(res);
                    Output respOutput = dataSerializer.GetAnswer(reqInput);

                    responseString = dataSerializer.Serialized(respOutput);//,\"Output\":{\"SumResult\":30.30,\"MulResult\":4,\"SortedInputs\":[1.0,1.01,2.02,4.0]}}";

                    break;
                case "/Stop":
                    listener.Stop();
                    Console.WriteLine("server stopped");
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
            }

            responseBody = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentType = "application/json; charset=utf-8";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = responseBody.Length;
            response.OutputStream.Write(responseBody, 0, responseBody.Length);

            response.Close();

            //// Construct a response.
            //string responseString = "<HTML><BODY>"+ method + "</BODY></HTML>";
            //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            //// Get a response stream and write the response to it.
            //response.ContentLength64 = buffer.Length;
            //System.IO.Stream output = response.OutputStream;
            //output.Write(buffer, 0, buffer.Length);
            //// You must close the output stream.
            //output.Close();
        }
    }
}
