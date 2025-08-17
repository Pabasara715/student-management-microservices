namespace StudentSystem.WebApp.ViewModels;

public class EnrollmentManagementDetailsViewModel
{
    public DateTime Date { get; set; }
    public EnrollmentDetails Enrollment { get; set; }

    public EnrollmentManagementDetailsViewModel()
    {
        Enrollment = new EnrollmentDetails();
    }
}

public class EnrollmentDetails
{
    public string Id { get; set; }
    public string Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Description { get; set; }
    public string ActualStartTime { get; set; }
    public string ActualEndTime { get; set; }
    public string Notes { get; set; }
    public CourseInfo Course { get; set; }
    public StudentInfo Student { get; set; }

    public EnrollmentDetails()
    {
        Course = new CourseInfo();
        Student = new StudentInfo();
    }
}

public class CourseInfo
{
    public string CourseCode { get; set; }
    public string Brand { get; set; }
    public string Type { get; set; }
}

public class StudentInfo
{
    public string StudentId { get; set; }
    public string Name { get; set; }
    public string TelephoneNumber { get; set; }
}