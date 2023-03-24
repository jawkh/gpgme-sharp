using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgpCombinedCrypto
{
    internal interface IGetSecrets
    {
        string GetSecretString(string SecretID);
    }
}
