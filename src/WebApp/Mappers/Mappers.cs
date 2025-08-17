using StudentSystem.StudentManagementAPI.Commands;
using StudentSystem.CourseManagement.Commands;
using StudentSystem.WebApp.ViewModels;

namespace StudentSystem.WebApp.Mappers;

public static class Mappers
{
    public static RegisterStudent MapToRegisterStudent(this StudentManagementNewViewModel source) => new RegisterStudent
    (
        Guid.NewGuid(),
        Guid.NewGuid().ToString("N"),
        source.FullName,
        source.DateOfBirth.Value,
        source.EmailAddress
    );

    public static CreateCourse MapToRegisterCourse(this CourseManagementNewViewModel source) => new CreateCourse(
        Guid.NewGuid(),
        source.Course.CourseCode,
        source.Course.CourseName,
        string.Empty,
        source.Course.Department,
        source.Course.CreditHours.Value
    );
}