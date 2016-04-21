using System.Diagnostics;
using Newtonsoft.Json;

namespace Blueclass.Wunderlist
{
    public class WunderlistResponse<T>
    {
        public bool IsSuccessStatusCode { get; set; }
        public T ResponseObject { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class WunderlistList
    {
        [JsonProperty("id")]
        public int Id { get; set; } 
        
        [JsonProperty("title")]
        public string Name { get; set; } 
    }

    [DebuggerDisplay("{" + nameof(Title) + "}")]
    public class WunderlistTask
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        // Maybe change to instance of list and tweak JsonSerialiser to use the id...
        [JsonProperty("list_id")]
        public int ListId { get; set; }

        [JsonProperty("revision")]
        public int Revision { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Content) + "}")]
    public class WunderlistNote
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("task_id")]
        public int TaskId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("revision")]
        public int Revision { get; set; }
    }

    public class ListPositions
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("values")]
        public int[] Order { get; set; }

        [JsonProperty("revision")]
        public int Revision { get; set; }

        // Maybe change to instance of list and tweak JsonSerialiser to use the id...
        [JsonProperty("list_id")]
        public int ListId { get; set; }

    }

    public class WunderlistFile
    {
        [JsonProperty("file_name")]
        public string FileName { get; set; }
    }

    public class WunderlistUpload
    {
        [JsonProperty("content_type")]
        public string ContentType { get; set; }
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        [JsonProperty("file_size")]
        public int FileSize { get; set; }
        [JsonProperty("md5sum")]
        public string Md5Sum { get; set; }
    }

    public class WunderlistUploadResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }
        [JsonProperty("part")]
        public WunderlistUploadResponseAuthenticationData Part { get; set; }
    }

    public class WunderlistUploadResponseAuthenticationData
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("authorization")]
        public string Authorization { get; set; }
    }
}