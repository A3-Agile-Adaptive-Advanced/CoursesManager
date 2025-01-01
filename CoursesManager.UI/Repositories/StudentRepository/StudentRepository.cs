﻿using System.Collections.ObjectModel;
using CoursesManager.UI.Models;
using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Repositories.Base;

namespace CoursesManager.UI.Repositories.StudentRepository;

public class StudentRepository : BaseRepository<Student>, IStudentRepository
{
    private readonly StudentDataAccess _studentDataAccess;

    private readonly ObservableCollection<Student> _students;

    private const string Cachekey = "studentsCache";

    private static readonly object SharedLock = new();

    public StudentRepository(StudentDataAccess studentDataAccess)
    {
        _studentDataAccess = studentDataAccess;

        try
        {
            _students = GlobalCache.Instance.Get(Cachekey) as ObservableCollection<Student> ?? SetupCache(Cachekey);
        }
        catch
        {
            _students = SetupCache(Cachekey);
        }
        finally
        {
            GetAll();
        }
    }

    public ObservableCollection<Student> GetAll()
    {
        lock (SharedLock)
        {
            if (_students.Count == 0)
            {
                _studentDataAccess.GetAll().ForEach(_students.Add);
            }

            return _students;
        }
    }

    public List<Student> GetNotDeletedStudents()
    {
        lock (SharedLock)
        {
            var col = GetAll();

            return col.Count == 0 ? _studentDataAccess.GetNotDeletedStudents() : col.Where(s => !s.IsDeleted).ToList();
        }
    }

    public List<Student> GetDeletedStudents()
    {
        lock (SharedLock)
        {
            var col = GetAll();

            return col.Count == 0 ? _studentDataAccess.GetNotDeletedStudents() : col.Where(s => s.IsDeleted).ToList();
        }
    }

    public Student? GetById(int id)
    {
        lock (SharedLock)
        {
            var item = _students.FirstOrDefault(s => s.Id == id);

            if (item is null)
            {
                item = _studentDataAccess.GetById(id);

                if (item is not null) _students.Add(item);
            }

            return item;
        }
    }

    public void Add(Student student)
    {
        lock (SharedLock)
        {
            _studentDataAccess.Add(student);

            _students.Add(student);
        }
    }

    public void Update(Student student)
    {
        lock (SharedLock)
        {
            _studentDataAccess.Update(student);

            var item = GetById(student.Id) ?? throw new InvalidOperationException($"Student with id: {student.Id} does not exist.");

            OverwriteItemInPlace(item, student);
        }
    }

    public void Delete(Student student)
    {
        Delete(student.Id);
    }

    public void Delete(int id)
    {
        lock (SharedLock)
        {
            _studentDataAccess.DeleteById(id);

            var item = GetById(id) ?? throw new InvalidOperationException($"Student with id: {id} does not exist.");

            _students.Remove(item);
        }
    }
}