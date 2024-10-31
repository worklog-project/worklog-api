using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Repository.implementation
{
    public class DailyRepository : IDailyRepository
    {
        private readonly string _connectionString;

        public DailyRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<EGIModel>> GetEGI(string egiName)
        {
            var egiList = new List<EGIModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT * FROM EGI
                    WHERE EGI_Name LIKE '%' + @egi + '%'";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@egi", (object)egiName ?? DBNull.Value);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var egi = new EGIModel
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            EGI = reader.GetString(reader.GetOrdinal("EGI_Name")),

                        };
                        egiList.Add(egi);
                    }
                }
            }
            return (egiList);
        }

        public async Task<IEnumerable<CodeNumberModel>> GetCodeNumber(string codeNumber, Guid egiID)
        {
            var codeNumberList = new List<CodeNumberModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT * FROM EGI_Code_Number
                    WHERE Code_Number LIKE '%' + @code + '%'
                    AND EGI_ID = @egiID";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@code", (object)codeNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@egiID", egiID);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var codeNumberModel = new CodeNumberModel
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            CodeNumber = reader.GetString(reader.GetOrdinal("Code_Number")),
                            EGIID = reader.GetGuid(reader.GetOrdinal("EGI_ID"))
                        };
                        codeNumberList.Add(codeNumberModel);
                    }
                }
            }
            return codeNumberList;
        }


    }
}
