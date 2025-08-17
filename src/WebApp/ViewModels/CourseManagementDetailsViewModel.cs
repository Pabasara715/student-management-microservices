namespace StudentSystem.WebApp.ViewModels;

public class CourseManagementDetailsViewModel
{
    public CourseDetails Course { get; set; }
    public string Owner { get; set; }

    public CourseManagementDetailsViewModel()
    {
        Course = new CourseDetails();
    }
}

public class CourseDetails
{
    public string CourseCode { get; set; }
    public string Brand { get; set; }
    public string Type { get; set; }
    public string OwnerId { get; set; }
}