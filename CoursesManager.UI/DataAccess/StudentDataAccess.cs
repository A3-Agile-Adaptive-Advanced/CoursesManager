using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using CoursesManager.UI.Database;

namespace CoursesManager.UI.DataAccess
{
    public class StudentDataAccess : BaseDataAccess<Student>
    {
        private readonly AddressDataAccess _addressDataAccess;

        public StudentDataAccess()
        {
            _addressDataAccess = new AddressDataAccess();
        }

        public List<Student> GetAll()
        {
            try
            {
                string procedureName = StoredProcedures.GetAllStudents;
                var results = ExecuteProcedure(procedureName);

                return results.Select(FillDataModel).ToList();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public List<Student> GetNotDeletedStudents()
        {
            try
            {
                string procedureName = StoredProcedures.GetNotDeletedStudents;
                var results = ExecuteProcedure(procedureName);

                return results.Select(FillDataModel).ToList();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public List<Student> GetDeletedStudents()
        {
            try
            {
                string procedureName = StoredProcedures.GetDeletedStudents;
                var results = ExecuteProcedure(procedureName);

                return results.Select(FillDataModel).ToList();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public void Add(Student student)
        {
            try
            {
                // Step 1: Add the address and get its ID
                int addressId = _addressDataAccess.Add(student.Address ?? throw new InvalidOperationException("Address can not be null."));
                if (addressId <= 0)
                {
                    throw new InvalidOperationException("Failed to create address.");
                }
                student.AddressId = addressId; // Assign FK to the student

                // Step 2: Add the student with the retrieved address ID
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_firstname", student.FirstName),
                    new MySqlParameter("@p_lastname", student.LastName),
                    new MySqlParameter("@p_email", student.Email),
                    new MySqlParameter("@p_phone", student.Phone),
                    new MySqlParameter("@p_address_id", addressId),
                    new MySqlParameter("@p_is_deleted", student.IsDeleted),
                    new MySqlParameter("@p_deleted_at", student.DeletedAt ?? (object)DBNull.Value),
                    new MySqlParameter("@p_created_at", DateTime.Now),
                    new MySqlParameter("@p_updated_at", DateTime.Now),
                    new MySqlParameter("@p_insertion", student.Insertion ?? (object)DBNull.Value),
                    new MySqlParameter("@p_date_of_birth", student.DateOfBirth.Date)                 };

                ExecuteNonProcedure(StoredProcedures.AddStudent, parameters);

                LogUtil.Log($"Student added successfully with Address ID: {addressId}");
            }
            catch (Exception ex)
            {
                LogUtil.Error($"Error adding student: {ex.Message}");
                throw;
            }
        }

        public Student? GetById(int id)
        {
            try
            {
                string procedureName = StoredProcedures.GetStudentById;
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_id", id) };
                var results = ExecuteProcedure(procedureName, parameters);

                return results.Select(FillDataModel).FirstOrDefault();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public void Update(Student student)
        {
            try
            {
                // Update the address first
                _addressDataAccess.Update(student.Address ?? throw new InvalidOperationException("Address can not be null"));

                int? addressId = student.AddressId;

                ExecuteNonProcedure(
                    StoredProcedures.UpdateStudent,
                    new MySqlParameter("@p_id", student.Id),
                    new MySqlParameter("@p_first_name", student.FirstName),
                    new MySqlParameter("@p_last_name", student.LastName),
                    new MySqlParameter("@p_email", student.Email),
                    new MySqlParameter("@p_phone", student.Phone),
                    new MySqlParameter("@p_address_id", addressId),
                    new MySqlParameter("@p_is_deleted", student.IsDeleted),
                    new MySqlParameter("@p_deleted_at", student.DeletedAt ?? (object)DBNull.Value),
                    new MySqlParameter("@p_created_at", DateTime.Now),
                    new MySqlParameter("@p_updated_at", DateTime.Now),
                    new MySqlParameter("@p_insertion", student.Insertion ?? (object)DBNull.Value),
                    new MySqlParameter("@p_date_of_birth", student.DateOfBirth)
                );

                LogUtil.Log($"Student: {student.Id} updated successfully");
            }
            catch (MySqlException ex)
            {
                LogUtil.Error($"MySQL error in Update: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                LogUtil.Error($"General error in Update: {ex.Message}");
                throw;
            }
        }

        public void DeleteById(int id)
        {
            try
            {
                ExecuteNonProcedure(StoredProcedures.DeleteStudent, new MySqlParameter("@p_id", id));
                LogUtil.Log($"Student ID: {id} deleted successfully.");
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"General error in Update: {ex.Message}");
                throw;
            }
        }

        protected Student FillDataModel(Dictionary<string, object?> row)
        {
            var student = new Student
            {
                Id = row.ContainsKey("id") && row["id"] != null ? Convert.ToInt32(row["id"]) : 0,
                FirstName = row.ContainsKey("firstname") && row["firstname"] != null
                    ? row["firstname"].ToString()
                    : string.Empty,
                LastName = row.ContainsKey("lastname") && row["lastname"] != null
                    ? row["lastname"].ToString()
                    : string.Empty,
                Email = row.ContainsKey("email") && row["email"] != null ? row["email"].ToString() : string.Empty,
                Phone = row.ContainsKey("phone") && row["phone"] != null ? row["phone"].ToString() : string.Empty,
                IsDeleted = row.ContainsKey("is_deleted") && row["is_deleted"] != null &&
                            Convert.ToBoolean(row["is_deleted"]),
                DeletedAt = row.ContainsKey("deleted_at") && row["deleted_at"] != null
                    ? Convert.ToDateTime(row["deleted_at"])
                    : (DateTime?)null,
                CreatedAt = row.ContainsKey("created_at") && row["created_at"] != null
                    ? Convert.ToDateTime(row["created_at"])
                    : DateTime.MinValue,
                UpdatedAt = row.ContainsKey("updated_at") && row["updated_at"] != null
                    ? Convert.ToDateTime(row["updated_at"])
                    : DateTime.MinValue,
                AddressId = row.ContainsKey("address_id") && row["address_id"] != null
                    ? Convert.ToInt32(row["address_id"])
                    : -1,
                DateOfBirth = row.ContainsKey("date_of_birth") && row["date_of_birth"] != null
                    ? Convert.ToDateTime(row["date_of_birth"])
                    : DateTime.MinValue,
                Insertion = row.ContainsKey("insertion") && row["insertion"] != null
                    ? row["insertion"].ToString()
                    : null
            };

            return student;
        }
    }
}