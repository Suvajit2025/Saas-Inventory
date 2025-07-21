using Invi.DataAccess;
using Serilog;
using System.Data;

namespace Invi.HelperClass
{
    public class DataService
    {
        private readonly AdoDataAccess _DTO;

        public DataService(AdoDataAccess dto)
        {
            _DTO = dto;
        }

        public async Task<int> AddAsync(string storedProcedure, Dictionary<string, object> parameters)
        {
            try
            {
                var parameterNames = parameters.Keys.ToArray();
                var parameterValues = parameters.Values.ToArray();

                return await _DTO.Int_ProcessAsync(storedProcedure, parameterNames, parameterValues);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: " + ex.Message, storedProcedure);
                throw new ApplicationException("An error occurred while adding the record.", ex);
            }
        }

        public async Task<DataTable> AddTableAsync(string storedProcedure, Dictionary<string, object> parameters)
        {
            try
            {
                var parameterNames = parameters.Keys.ToArray();
                var parameterValues = parameters.Values.ToArray();

                return await _DTO.Dt_ProcessAsync(storedProcedure, parameterNames, parameterValues);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: " + ex.Message, storedProcedure);
                throw new ApplicationException("An error occurred while adding the record.", ex);
            }
        }

        public async Task<int> UpdateAsync(string storedProcedure, Dictionary<string, object> parameters)
        {
            try
            {
                var parameterNames = parameters.Keys.ToArray();
                var parameterValues = parameters.Values.ToArray();

                return await _DTO.Int_ProcessAsync(storedProcedure, parameterNames, parameterValues);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: " + ex.Message, storedProcedure);
                throw new ApplicationException("An error occurred while updating the record.", ex);
            }
        }

        public async Task<DataTable> UpdateTableAsync(string storedProcedure, Dictionary<string, object> parameters)
        {
            try
            {
                var parameterNames = parameters.Keys.ToArray();
                var parameterValues = parameters.Values.ToArray();

                return await _DTO.Dt_ProcessAsync(storedProcedure, parameterNames, parameterValues);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: " + ex.Message, storedProcedure);
                throw new ApplicationException("An error occurred while updating the record.", ex);
            }
        }

        public async Task<int> DeleteAsync(string storedProcedure, Dictionary<string, object> parameters)
        {
            try
            {
                var parameterNames = parameters.Keys.ToArray();
                var parameterValues = parameters.Values.ToArray();

                return await _DTO.Int_ProcessAsync(storedProcedure, parameterNames, parameterValues);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: " + ex.Message, storedProcedure);
                throw new ApplicationException("An error occurred while deleting the record.", ex);
            }
        }

        public async Task<DataTable> GetDataAsync(string storedProcedure, Dictionary<string, object> parameters)
        {
            try
            {
                var parameterNames = parameters.Keys.ToArray();
                var parameterValues = parameters.Values.ToArray();

                return await _DTO.Dt_ProcessAsync(storedProcedure, parameterNames, parameterValues);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: " + ex.Message, storedProcedure);
                throw new ApplicationException("An error occurred while retrieving records.", ex);
            }
        }

        public async Task<DataSet> GetAllDatasetAsync(string storedProcedure, Dictionary<string, object> parameters)
        {
            try
            {
                var parameterNames = parameters.Keys.ToArray();
                var parameterValues = parameters.Values.ToArray();

                return await _DTO.Ds_ProcessAsync(storedProcedure, parameterNames, parameterValues);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: " + ex.Message, storedProcedure);
                throw new ApplicationException("An error occurred while retrieving the dataset.", ex);
            }
        }

        public async Task<int> AddWithTVPAsync(string storedProcedure, string tvpParameterName, string tableName, DataTable tvpData)
        {
            try
            {
                return await _DTO.Int_ProcessWithTVPAsync(storedProcedure, tvpParameterName, tableName, tvpData);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: " + ex.Message, storedProcedure);
                throw new ApplicationException("An error occurred while adding records via the TVP.", ex);
            }
        }

        public async Task<int> Int_ProcessWithMultipleTVPsAsync(string storedProcedure, Dictionary<string, object> parameters, Dictionary<string, (string TableName, DataTable Data)> tvpParameters)
        {
            try
            {
                return await _DTO.Int_ProcessWithMultipleTVPsAsync(storedProcedure, parameters, tvpParameters);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: " + ex.Message, storedProcedure);
                throw new ApplicationException("An error occurred while adding records via the TVP.", ex);
            }
        }
    }
}
