﻿using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using CoursesManager.UI.Database;

namespace CoursesManager.UI.DataAccess
{
    public class AddressDataAccess : BaseDataAccess<Address>
    {
        public List<Address> GetAll()
        {
            try
            {
                string procedureName = StoredProcedures.GetAllAddresses;
                var results = ExecuteProcedure(procedureName);
                return results.Select(FillDataModel).ToList();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public Address? GetById(int id)
        {
            try
            {
                string procedureName = StoredProcedures.GetAddressById;
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_id", id) };
                var results = ExecuteProcedure(procedureName, parameters);
                return results.Select(FillDataModel).FirstOrDefault();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public dynamic Add(Address address)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_country", address.Country),
                    new MySqlParameter("@p_zipcode", address.ZipCode),
                    new MySqlParameter("@p_city", address.City),
                    new MySqlParameter("@p_street", address.Street),
                    new MySqlParameter("@p_house_number", address.HouseNumber),
                    new MySqlParameter("@p_house_number_extension",
                        address.HouseNumberExtension ?? (object)DBNull.Value),
                    new MySqlParameter("@p_created_at", DateTime.Now),
                    new MySqlParameter("@p_updated_at", DateTime.Now),
                    new MySqlParameter("@p_id", MySqlDbType.Int32)
                    {
                        Direction = ParameterDirection.Output
                    }
                };

                ExecuteNonProcedure(StoredProcedures.AddAddress, parameters);

                int addressId = Convert.ToInt32(parameters.Single(p => p.ParameterName == "@p_id").Value);
                LogUtil.Log($"Address created successfully with ID: {addressId}");
                return addressId;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public void Update(Address address)
        {
            try
            {
                string procedureName = StoredProcedures.UpdateAddress;
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_id", address.Id),
                    new MySqlParameter("@p_country", address.Country),
                    new MySqlParameter("@p_zipcode", address.ZipCode),
                    new MySqlParameter("@p_city", address.City),
                    new MySqlParameter("@p_street", address.Street),
                    new MySqlParameter("@p_house_number", address.HouseNumber),
                    new MySqlParameter("@p_house_number_extension", address.HouseNumberExtension ?? (object)DBNull.Value),
                    new MySqlParameter("@p_updated_at", DateTime.Now)
                };

                ExecuteNonProcedure(procedureName, parameters);
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public void Delete(int id)
        {
            try
            {
                string procedureName = StoredProcedures.DeleteAddress;
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_id", id) };
                ExecuteNonProcedure(procedureName, parameters);
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        protected Address FillDataModel(Dictionary<string, object> row)
        {
            try
            {
                return new Address
                {
                    Id = Convert.ToInt32(row["id"]),
                    Country = row["country"].ToString(),
                    ZipCode = row["zipcode"].ToString(),
                    City = row["city"].ToString(),
                    Street = row["street"].ToString(),
                    HouseNumber = row["house_number"].ToString(),
                    HouseNumberExtension = row["house_number_extension"]?.ToString(),
                    CreatedAt = Convert.ToDateTime(row["created_at"]),
                    UpdatedAt = Convert.ToDateTime(row["updated_at"])
                };
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }
    }
}
