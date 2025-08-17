using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentSystem.WebApp.ViewModels;

public class CourseManagementNewViewModel
{
    public CourseInsertModel Course { get; set; }

    [Display(Name = "Student")]
    public string SelectedStudentId { get; set; }
    public IEnumerable<SelectListItem> Students { get; set; }

    public CourseManagementNewViewModel()
    {
        Course = new CourseInsertModel();
        Students = new List<SelectListItem>();
    }
}

// In ViewModels/CourseManagementNewViewModel.cs

public class CourseInsertModel
{
    [Display(Name = "Course Code")]
    public string CourseCode { get; set; }

    [Display(Name = "Course Name")]
    public string CourseName { get; set; }

    [Display(Name = "Department")]
    public string Department { get; set; }

    // Add this property
    [Required]
    [Range(1, 10)]
    [Display(Name = "Credit Hours")]
    public int? CreditHours { get; set; }
}