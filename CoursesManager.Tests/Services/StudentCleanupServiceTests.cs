using System.Collections.ObjectModel;
using CoursesManager.UI.Models;
using Moq;
using NUnit.Framework;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Services;

namespace CoursesManager.Tests.Services
{
    [TestFixture]
    public class StudentCleanupServiceTests
    {
        private Mock<IStudentRepository> _mockStudentRepository;
        private StudentCleanupService _cleanupService;

        [SetUp]
        public void SetUp()
        {
            _mockStudentRepository = new Mock<IStudentRepository>();
            _cleanupService = new StudentCleanupService(_mockStudentRepository.Object);
        }

        [Test]
        public void CleanupDeletedStudents_DeletesStudentsDeletedMoreThanTwoYearsAgo()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { Id = 1, IsDeleted = false, DeletedAt = DateTime.Now.AddYears(-3) },
                new Student { Id = 2, IsDeleted = true, DeletedAt = DateTime.Now.AddYears(-1) },
                new Student { Id = 3, IsDeleted = true, DeletedAt = DateTime.Now.AddYears(-4) }
            };

            _mockStudentRepository.Setup(repo => repo.GetDeletedStudents()).Returns(students);

            // Act
            _cleanupService.CleanupDeletedStudents();

            // Assert
            _mockStudentRepository.Verify(repo => repo.Delete(1), Times.Never);
            _mockStudentRepository.Verify(repo => repo.Delete(3), Times.Once);
            _mockStudentRepository.Verify(repo => repo.Delete(2), Times.Never);
        }

        [Test]
        public void CleanupDeletedStudents_NoStudentsDeletedIfNoneOlderThanTwoYears()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { Id = 1, IsDeleted = true, DeletedAt = DateTime.Now.AddYears(-1) },
                new Student { Id = 2, IsDeleted = true, DeletedAt = DateTime.Now.AddMonths(-6) }
            };
            _mockStudentRepository.Setup(repo => repo.GetDeletedStudents()).Returns(students);

            // Act
            _cleanupService.CleanupDeletedStudents();

            // Assert
            _mockStudentRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void CleanupDeletedStudents_ThrowsExceptionIfRepositoryFailsToRetrieveDeletedStudents()
        {
            // Arrange
            _mockStudentRepository.Setup(repo => repo.GetDeletedStudents()).Throws(new InvalidOperationException("Database error"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _cleanupService.CleanupDeletedStudents());
        }

        [Test]
        public void CleanupDeletedStudents_ThrowsExceptionIfRepositoryFailsToDeleteStudent()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { Id = 1, IsDeleted = true, DeletedAt = DateTime.Now.AddYears(-3) }
            };
            _mockStudentRepository.Setup(repo => repo.GetDeletedStudents()).Returns(students);
            _mockStudentRepository.Setup(repo => repo.Delete(It.IsAny<int>())).Throws(new InvalidOperationException("Database error"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _cleanupService.CleanupDeletedStudents());
        }
    }
}
