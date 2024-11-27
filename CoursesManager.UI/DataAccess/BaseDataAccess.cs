﻿using MySql.Data.MySqlClient;
using System.Reflection;
using CoursesManager.MVVM.Env;
using CoursesManager.UI.Models;

namespace CoursesManager.UI.DataAccess;

public abstract class BaseDataAccess<T> where T : new()
{
    private readonly string _connectionString = EnvManager<EnvModel>.Values.ConnectionString;
    protected readonly string _modelTableName;

    protected MySqlConnection GetConnection() => new(_connectionString);

    /// <inheritdoc />
    protected BaseDataAccess() : this(typeof(T).Name.ToLower()) { }

    /// <summary>
    /// Sets up basic functionality of the Data access layer.
    /// </summary>
    /// <param name="modelTableName">Name of the table that is represented with this data access object.</param>
    protected BaseDataAccess(string modelTableName)
    {
        _modelTableName = modelTableName;
    }

    public List<T> FetchAll()
    {
        return FetchAll($"SELECT * FROM `{_modelTableName}`;");
    }

    public List<T> FetchAll(string query, params MySqlParameter[]? parameters)
    {
        List<T> result = new();
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        using var mySqlConnection = GetConnection();
        using MySqlCommand mySqlCommand = new($"{query};", mySqlConnection);

        if (parameters is not null)
        {
            mySqlCommand.Parameters.AddRange(parameters);
        }

        try
        {
            mySqlConnection.Open();

            using MySqlDataReader mySqlReader = mySqlCommand.ExecuteReader();
            while (mySqlReader.Read())
            {
                result.Add(FillModel(mySqlReader, properties));
            }
        }
        catch (MySqlException exception)
        {
            LogUtil.Error(exception.Message);
        }
        catch (InvalidCastException exception)
        {
            LogUtil.Error(exception.Message);
        }

        return result;
    }

    public T? FetchOneById(int id)
    {
        return FetchAll(
            $"SELECT * FROM `{_modelTableName}` WHERE ID = @ID;",
            [new MySqlParameter("@ID", id)]
        ).FirstOrDefault();
    }

    protected bool InsertRow(Dictionary<string, object> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfZero(data.Count);

        string columns = string.Join(", ", data.Keys.Select(k => $"`{k}`"));
        string parameters = string.Join(", ", data.Keys.Select(k => $"@{k}"));
        string query = $"INSERT INTO `{_modelTableName}` ({columns}) VALUES ({parameters});";

        return ExecuteNonQuery(query, data);
    }

    protected bool UpdateRow(Dictionary<string, object> data, string whereClause, params MySqlParameter[]? parameters)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfZero(data.Count);

        if (string.IsNullOrWhiteSpace(whereClause))
        {
            LogUtil.Error("Where clause cannot be null or empty.");
            return false;
        }

        string setClause = string.Join(", ", data.Keys.Select(k => $"`{k}` = @{k}"));
        string query = $"UPDATE `{_modelTableName}` SET {setClause} WHERE {whereClause};";

        List<MySqlParameter> allParameters = data.Select(kvp => new MySqlParameter($"@{kvp.Key}", kvp.Value ?? DBNull.Value)).ToList();

        if (parameters is not null)
        {
            allParameters.AddRange(parameters);
        }

        return ExecuteNonQuery(query, allParameters.ToArray());
    }

    protected bool DeleteRow(string whereClause, params MySqlParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(whereClause))
        {
            LogUtil.Error("Where clause cannot be null or empty.");
            return false;
        }

        string query = $"DELETE FROM `{_modelTableName}` WHERE {whereClause};";
        return ExecuteNonQuery(query, parameters);
    }

    protected T FillModel(MySqlDataReader mySqlReader, PropertyInfo[] properties)
    {
        T model = new();

        foreach (var property in properties)
        {
            if (!HasColumn(mySqlReader, property.Name) || mySqlReader[property.Name] is DBNull)
            {
                continue;
            }

            try
            {
                property.SetValue(model, Convert.ChangeType(mySqlReader[property.Name], property.PropertyType));
            }
            catch (Exception exception)
            {
                throw new InvalidCastException($"Error converting column '{property.Name}' to property '{property.Name}' of type '{property.PropertyType}'.", exception);
            }
        }

        return model;
    }

    public bool ExecuteNonQuery(string query, params MySqlParameter[]? parameters)
    {
        using var mySqlConnection = GetConnection();
        using var mySqlCommand = new MySqlCommand(query, mySqlConnection);

        if (parameters is not null)
        {
            mySqlCommand.Parameters.AddRange(parameters);
        }

        try
        {
            mySqlConnection.Open();
            mySqlCommand.ExecuteNonQuery();
        }
        catch (MySqlException exception)
        {
            LogUtil.Error(exception.Message);
            throw;
        }

        return true;
    }

    public bool ExecuteNonQuery(string query, Dictionary<string, object> data)
    {
        MySqlParameter[] parameters = data.Select(kvp => new MySqlParameter($"@{kvp.Key}", kvp.Value ?? DBNull.Value)).ToArray();
        return ExecuteNonQuery(query, parameters);
    }

    private bool HasColumn(MySqlDataReader mySqlReader, string columnName)
    {
        for (var i = 0; i < mySqlReader.FieldCount; i++)
        {
            if (mySqlReader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}