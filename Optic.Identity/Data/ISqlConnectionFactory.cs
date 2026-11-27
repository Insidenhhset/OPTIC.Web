
using Microsoft.Data.SqlClient;

namespace Optic.Identity.Data
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
