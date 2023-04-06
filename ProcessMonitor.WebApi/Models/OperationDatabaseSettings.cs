namespace ProcessMonitor.WebApi.Models
{
    public class OperationDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string OperationsContainerName { get; set; } = null!;
    }
}
