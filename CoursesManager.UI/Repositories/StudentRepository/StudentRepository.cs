﻿using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.StudentRepository;

public class StudentRepository : IStudentRepository
{
    public List<Student> GetAll()
    {
        throw new NotImplementedException();
    }
    // I don't wanna ruin other view models in the app so i made this temporary 
    public List<Student> GetAllStudents()
    {
        throw new NotImplementedException();

    }

    public List<Student> RefreshAll()
    {
        throw new NotImplementedException();
    }

    public Student? GetById(int id)
    {
        throw new NotImplementedException();
    }

    public void Add(Student data)
    {
        throw new NotImplementedException();
    }

    public void Update(Student data)
    {
        throw new NotImplementedException();
    }

    public void Delete(Student data)
    {
        throw new NotImplementedException();
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }
}