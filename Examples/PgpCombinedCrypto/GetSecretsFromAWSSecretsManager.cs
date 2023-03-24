using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.SecretsManager.Extensions.Caching;
//using Amazon.SecretsManager.Extensions.Caching.SecretsManagerCache;

namespace PgpCombinedCrypto
{
    internal class GetSecretsFromAWSSecretsManager : IGetSecrets
    {
        private static SecretsManagerCache cache = new SecretsManagerCache();
        private async Task<string> GetSecretStringAsync(string SecretID)
        {
            string secrets;
            
            secrets = await cache.GetSecretString(SecretID);

            return secrets;
        }
                

        public string GetSecretString(string SecretID) {

            // Start a task - calling an async function in this example
            Task<string> callTask = Task.Run(() => GetSecretStringAsync(SecretID));
            // Wait for it to finish
            callTask.Wait();
            // Get the result
            return callTask.Result;
        }
    }
}
