namespace StudentSystem.WebApp.ViewModels;

// An enum to represent the loyalty levels
public enum LoyaltyLevel
{
    Bronze,
    Silver,
    Gold
}

public class StudentManagementDetailsViewModel
{
    public StudentDetails Student { get; set; }

    public StudentManagementDetailsViewModel()
    {
        Student = new StudentDetails();
    }
}

public class StudentDetails
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string TelephoneNumber { get; set; }
    public string EmailAddress { get; set; }

    // Nullable enum to match the view's '.HasValue' check
    public LoyaltyLevel? LoyaltyLevel { get; set; }
}