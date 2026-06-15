using System;
using System.Collections.Generic;

namespace GPQ.Client.Models
{
    public class KeyValuePairModel
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
    }

    public class RequestModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "New Request";
        public string Method { get; set; } = "GET";
        public string Url { get; set; } = "";
        public List<KeyValuePairModel> Params { get; set; } = new();
        public List<KeyValuePairModel> Headers { get; set; } = new();
        public string Body { get; set; } = "";
        public string BodyMode { get; set; } = "none";
        public List<KeyValuePairModel> FormData { get; set; } = new();
        public List<KeyValuePairModel> UrlEncodedData { get; set; } = new();
    }

    public class ProxyRequest
    {
        public string Method { get; set; } = "GET";
        public string Url { get; set; } = "";
        public List<KeyValuePairModel> Headers { get; set; } = new();
        public string Body { get; set; } = "";
        public string BodyMode { get; set; } = "none";
        public List<KeyValuePairModel> FormData { get; set; } = new();
        public List<KeyValuePairModel> UrlEncodedData { get; set; } = new();
    }

    public class ProxyResponse
    {
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
        public List<KeyValuePairModel> Headers { get; set; } = new();
        public string Body { get; set; } = string.Empty;
        public long ElapsedTimeMs { get; set; }
        public long SizeInBytes { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
