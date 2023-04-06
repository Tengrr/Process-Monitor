namespace ProcesssMonitor.FunctionApp
{
    using Newtonsoft.Json;

    public class Operation
    {
        [JsonProperty(PropertyName = "id")]
        public string? id { get; set; }

        public string name { get; set; } = null!;

        public int pid { get; set; } = int.MinValue;

        public string? processList { get; set; }
    }
}
