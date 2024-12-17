using MySql.Data.MySqlClient;
using System.Reflection;
using CoursesManager.MVVM.Env;
using CoursesManager.UI.Models;
using System.Data;
using CoursesManager.UI.Service;

namespace CoursesManager.UI.DataAccess;

public abstract class BaseDataAccess<T>(string? modelTableName = null) where T : new()
{
    protected readonly string _dbTableName = modelTableName ?? typeof(T).Name.ToLower();

    public List<T> FetchAll() => FetchAll($"SELECT * FROM `{_dbTableName}`;");

    public List<T> FetchAll(string query, params MySqlParameter[]? parameters)
    {
        List<T> result = new();
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        using MySqlConnection mySqlConnection = GetConnection();
        using MySqlCommand mySqlCommand = new($"{query};", mySqlConnection);

        if (parameters is { Length: > 0 })
            mySqlCommand.Parameters.AddRange(parameters);

        try
        {
            mySqlConnection.Open();

            using MySqlDataReader mySqlReader = mySqlCommand.ExecuteReader();
            while (mySqlReader.Read())
                result.Add(FillModel(mySqlReader, properties));
        }
        catch (MySqlException exception)
        {
            LogUtil.Error(exception.Message);
        }

        return result;
    }

    public T? FetchOneById(int id) => 
        FetchAll($"SELECT * FROM `{_dbTableName}` WHERE ID = @ID;", new MySqlParameter("@ID", id)).FirstOrDefault();

    public dynamic InsertRow(T model) => InsertRow(ModelToDictionary(model));
    public dynamic InsertRow(Dictionary<string, object> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfZero(data.Count);

        string columns = string.Join(", ", data.Keys.Select(k => $"`{k}`"));
        string parameters = string.Join(", ", data.Keys.Select(k => $"@{k}"));
        string query = $"INSERT INTO `{_dbTableName}` ({columns}) VALUES ({parameters});";

        return ExecuteNonQuery(query, DictionaryToParameters(data)) ? GetLastInsertedId() : false;
    }

    public dynamic UpdateRow(T model, string whereClause, params MySqlParameter[]? parameters) =>
        UpdateRow(ModelToDictionary(model), whereClause, parameters);

    public dynamic UpdateRow(Dictionary<string, object?> data, string whereClause, params MySqlParameter[]? parameters)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfZero(data.Count);

        if (string.IsNullOrWhiteSpace(whereClause))
        {
            LogUtil.Error("Where clause cannot be null or empty.");
            return false;
        }

        string setClause = string.Join(", ", data.Keys.Select(k => $"`{k}` = @{k}"));
        string query = $"UPDATE `{_dbTableName}` SET {setClause} WHERE {whereClause};";

        List<MySqlParameter> allParameters = data.Select(kvp => new MySqlParameter($"@{kvp.Key}", kvp.Value ?? DBNull.Value)).ToList();

        var allParams = DictionaryToParameters(data).ToList();
        if (parameters is { Length: > 0 })
            allParams.AddRange(parameters);

        return ExecuteNonQuery(query, [.. allParameters]) ? GetLastInsertedId() : false;
    }

    public dynamic DeleteRow(string whereClause, params MySqlParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(whereClause))
        {
            LogUtil.Error("Where clause cannot be null or empty.");
            return false;
        }

        return ExecuteNonQuery($"DELETE FROM `{_dbTableName}` WHERE {whereClause};", parameters);
    }

    /// <summary>
    /// Executes a stored procedure that returns one or more result sets.
    /// Returns the results as a list of dictionaries keyed by column name.
    /// </summary>
    public List<Dictionary<string, object?>> ExecuteProcedure(string procedureName, params MySqlParameter[]? parameters)
    {
        using MySqlConnection mySqlConnection = GetConnection();
        using MySqlCommand mySqlCommand = new(procedureName, mySqlConnection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters is { Length: > 0 })
            mySqlCommand.Parameters.AddRange(parameters);

        try
        {
            mySqlConnection.Open();
            using MySqlDataReader reader = mySqlCommand.ExecuteReader();

            List<Dictionary<string, object?>> results = [];
            while (reader.Read())
            {
                Dictionary<string, object?> row = [];
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                
                results.Add(row);
            }

            return results;
        }
        catch (MySqlException exception)
        {
            LogUtil.Error($"Error executing procedure '{procedureName}': {exception.Message}");
            throw;
        }
    }

    /// <summary>
    /// Executes a stored procedure that does not return a result set (e.g., INSERT, UPDATE, DELETE).
    /// Returns true if execution succeeds.
    /// </summary>
    public dynamic ExecuteNonProcedure(string procedureName, params MySqlParameter[]? parameters)
    {
        using MySqlConnection mySqlConnection = GetConnection();
        using MySqlCommand mySqlCommand = new(procedureName, mySqlConnection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters is { Length: > 0 })
            mySqlCommand.Parameters.AddRange(parameters);


        try
        {
            mySqlConnection.Open();
            mySqlCommand.ExecuteNonQuery();

            return mySqlCommand.ExecuteNonQuery() > 0 ? GetLastInsertedId() : false;
        }
        catch (MySqlException exception)
        {
            LogUtil.Error($"Error executing procedure '{procedureName}': {exception.Message}");
            throw;
        }
    }

    private int GetLastInsertedId()
    {
        using var mySqlConnection = GetConnection();
        using var mySqlCommand = new MySqlCommand("SELECT LAST_INSERT_ID()", mySqlConnection);

        mySqlConnection.Open();
        return Convert.ToInt32(mySqlCommand.ExecuteScalar());
    }

    protected MySqlConnection GetConnection()
    {
        var connectionString = EnvManager<EnvModel>.Values.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("De ConnectionString is leeg of ongeldig.");

        return new MySqlConnection(connectionString);
    }

    protected T FillModel(MySqlDataReader mySqlReader, PropertyInfo[] properties)
    {
        T model = new();
        foreach (var property in properties)
        {
            if (!HasColumn(mySqlReader, property.Name) || mySqlReader[property.Name] is DBNull) continue;

            try
            {
                object? value = mySqlReader[property.Name];
                property.SetValue(model, Convert.ChangeType(value, property.PropertyType));
            }
            catch (Exception exception)
            {
                throw new InvalidCastException($"Error converting column '{property.Name}' to property '{property.Name}' of type '{property.PropertyType}'.", exception);
            }
        }

        return model;
    }

    /// <summary>Executes a non-query command (INSERT, UPDATE, DELETE).</summary>
    private bool ExecuteNonQuery(string query, params MySqlParameter[]? parameters)
    {
        using var mySqlConnection = GetConnection();
        using var mySqlCommand = new MySqlCommand(query, mySqlConnection);

        if (parameters is { Length: > 0 })
            mySqlCommand.Parameters.AddRange(parameters);

        try
        {
            mySqlConnection.Open();
            mySqlCommand.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException exception)
        {
            LogUtil.Error(exception.Message);
            return false;
        }
    }

    private static Dictionary<string, object> ModelToDictionary(T model)
    {
        Dictionary<string, object?> dict = [];
        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            dict[prop.Name] = prop.GetValue(model) ?? DBNull.Value;

        return dict;
    }

    private static MySqlParameter[] DictionaryToParameters(Dictionary<string, object> data) =>
        data.Select(kvp => new MySqlParameter($"@{kvp.Key}", kvp.Value ?? DBNull.Value)).ToArray();

    private static bool HasColumn(MySqlDataReader mySqlReader, string columnName)
    {
        for (var i = 0; i < mySqlReader.FieldCount; i++)
            if (mySqlReader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

}