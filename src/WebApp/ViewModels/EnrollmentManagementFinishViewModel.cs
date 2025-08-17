using System;
using System.ComponentModel.DataAnnotations;

namespace StudentSystem.WebApp.ViewModels;

public class EnrollmentManagementFinishViewModel
{
    // Hidden fields for the form to track the enrollment
    public string Id { get; set; }
    public DateTime Date { get; set; }

    [Required]
    [Display(Name = "Actual Start Time (HH:mm)")]
    public string ActualStartTime { get; set; }

    [Required]
    [Display(Name = "Actual End Time (HH:mm)")]
    public string ActualEndTime { get; set; }

    [DataType(DataType.MultilineText)]
    public string Notes { get; set; }
}