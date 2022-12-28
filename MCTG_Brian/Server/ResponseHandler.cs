namespace MCTG_Brian.Server
{
    class ResponseHandler
    {
        public int Status { get; set; }
        public string ContentType { get; set; }
        public string Body { get; set; }

        public ResponseHandler(int statusCode, string contentType, string body)
        {
            Status = statusCode;
            ContentType = contentType;
            Body = body;
        }

        public string HttpResponseToString()
        {
            return $"HTTP/1.1 {Status} OK\r\nContent-Type: {ContentType}\r\nContent-Length: {Body.Length}\r\n\r\n{Body}";
        }

    }
}
