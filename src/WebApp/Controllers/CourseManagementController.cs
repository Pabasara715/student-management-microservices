using Microsoft.AspNetCore.Mvc;
using StudentSystem.WebApp.Models;
using StudentSystem.WebApp.RESTClients;
using StudentSystem.WebApp.ViewModels;

namespace StudentSystem.WebApp.Controllers;

public class CourseManagementController : Controller
{
    private readonly ICourseManagementAPI _courseManagementAPI;

    public CourseManagementController(ICourseManagementAPI courseManagementAPI)
    {
        _courseManagementAPI = courseManagementAPI;
    }

    public async Task<IActionResult> Index()
    {
        var model = new CourseManagementViewModel
        {
            Courses = await _courseManagementAPI.GetCourses()
        };
        return View(model);
    }

    public IActionResult New()
    {
        var model = new CourseManagementNewViewModel();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> New(CourseManagementNewViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {

                await _courseManagementAPI.RegisterCourse(new Course
                {
                    CourseCode = model.Course.CourseCode,
                    CourseName = model.Course.CourseName,
                    Department = model.Course.Department,
                    CreditHours = model.Course.CreditHours.Value
                });
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the course.");
            }
        }
        return View(model);
    }
}