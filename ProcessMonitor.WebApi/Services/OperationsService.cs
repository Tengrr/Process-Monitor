namespace ProcessMonitor.WebApi.Services
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Options;
    using ProcessMonitor.WebApi.Models;
    using System.Net;
    using System.Security.Cryptography;

    public class OperationsService
    {
        private Container _operationsContainer;

        public OperationsService(
            IOptions<OperationDatabaseSettings> operationDatabaseSettings)
        {
            var cosmosClient = new CosmosClient(operationDatabaseSettings.Value.ConnectionString);

            var cosmosDatabase = cosmosClient.GetDatabase(
                operationDatabaseSettings.Value.DatabaseName);

            _operationsContainer = cosmosDatabase.GetContainer(operationDatabaseSettings.Value.OperationsContainerName);
        }

        public async Task<List<Operation>> GetAsync()
        {
            string queryString = "SELECT * FROM c";
            var query = _operationsContainer.GetItemQueryIterator<Operation>(new QueryDefinition(queryString));
            List<Operation> results = new List<Operation>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<Operation?> GetAsync(string id)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{id}'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Operation> queryResultSetIterator = _operationsContainer.GetItemQueryIterator<Operation>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Operation> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Operation operation in currentResultSet)
                {
                    return operation;
                }
            }
            return null;
        }

        public async Task CreateAsync(Operation newOperation)
        {
            try
            {
                newOperation.id = GetRandomString(16);
                await _operationsContainer.CreateItemAsync<Operation>(newOperation, new PartitionKey(newOperation.name));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine("Item in database with id: {0} already exists\n", newOperation.id);
            }

        }

        /// <summary>
        /// 产生指定长度的随机字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = "0123456789abcdefghijklmnopqrstuvwxyz";
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }

    }
}
