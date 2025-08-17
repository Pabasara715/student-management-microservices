using StudentSystem.NotificationService.Events;
using StudentSystem.NotificationService.Model;
using StudentSystem.NotificationService.NotificationChannels;
using StudentSystem.NotificationService.Repositories;
using StudentSystem.Infrastructure.Messaging;
using Newtonsoft.Json.Linq;
using Serilog;
using Microsoft.Extensions.Hosting;
using System.Text; 

namespace StudentSystem.NotificationService;

public class NotificationWorker : IHostedService, IMessageHandlerCallback
{
    private readonly IMessageHandler _messageHandler;
    private readonly INotificationRepository _repo;
    private readonly IEmailNotifier _emailNotifier;

    public NotificationWorker(IMessageHandler messageHandler, INotificationRepository repo, IEmailNotifier emailNotifier)
    {
        _messageHandler = messageHandler;
        _repo = repo;
        _emailNotifier = emailNotifier;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _messageHandler.Start(this);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _messageHandler.Stop();
        return Task.CompletedTask;
    }

    public async Task<bool> HandleMessageAsync(string messageType, string message)
    {
        try
        {
            JObject messageObject = MessageSerializer.Deserialize(message);
            switch (messageType)
            {
                case "StudentRegistered":
                    await HandleAsync(messageObject.ToObject<StudentRegistered>());
                    break;
                case "StudentEnrolled": // Use the event name from EnrollmentManagementAPI
                    await HandleAsync(messageObject.ToObject<EnrollmentPlanned>());
                    break;
                case "CourseCompleted": // Use the event name from EnrollmentManagementAPI
                    await HandleAsync(messageObject.ToObject<CourseCompleted>()); // FIX APPLIED HERE
                    break;
                case "DayHasPassed":
                    await HandleAsync(messageObject.ToObject<DayHasPassed>());
                    break;
                default:
                    Log.Warning("Unknown message type: {MessageType}", messageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error while handling {messageType} event.");
        }

        return true;
    }

    private async Task HandleAsync(StudentRegistered sr)
    {
        var student = new Student
        {
            StudentId = sr.StudentId,
            FullName = sr.FullName,
            EmailAddress = sr.EmailAddress
        };

        Log.Information("Register student: {Id}, {Name}, {Email}", student.StudentId, student.FullName, student.EmailAddress);
        await _repo.RegisterStudentAsync(student);
    }

    private async Task HandleAsync(EnrollmentPlanned ep)
    {
        var enrollment = new Enrollment
        {
            EnrollmentId = ep.EnrollmentId.ToString(),
            StudentId = ep.StudentId,
            CourseCode = ep.CourseCode,
            StartTime = ep.EnrollmentDate,
            Description = $"Enrollment for {ep.CourseCode}"
        };

        Log.Information("Register Enrollment: {Id}, {StudentId}, {CourseCode}, {StartTime}",
            enrollment.EnrollmentId, enrollment.StudentId, enrollment.CourseCode, enrollment.StartTime);

        await _repo.RegisterEnrollmentAsync(enrollment);
    }

    private async Task HandleAsync(CourseCompleted ef) 
    {
        Log.Information("Remove completed Enrollment: {Id}", ef.EnrollmentId);
        await _repo.RemoveEnrollmentsAsync(new[] { ef.EnrollmentId.ToString() });
    }

    private async Task HandleAsync(DayHasPassed dhp)
    {
        DateTime today = DateTime.Now.Date;
        IEnumerable<Enrollment> enrollmentsToNotify = await _repo.GetEnrollmentsForTodayAsync(today);

        foreach (var enrollmentsPerStudent in enrollmentsToNotify.GroupBy(e => e.StudentId))
        {
            string studentId = enrollmentsPerStudent.Key;
            Student student = await _repo.GetStudentAsync(studentId);

            if (student == null) continue;

            StringBuilder body = new StringBuilder();
            body.AppendLine($"Dear {student.FullName},\n");
            body.AppendLine($"This is a reminder that you have classes scheduled for tomorrow:\n");

            foreach (Enrollment enrollment in enrollmentsPerStudent)
            {
                body.AppendLine($"- {enrollment.StartTime:dd-MM-yyyy HH:mm} : {enrollment.CourseCode}");
            }

            body.AppendLine($"\nPlease ensure you attend your scheduled classes.\n");
            body.AppendLine($"Greetings,\n");
            body.AppendLine($"The Student Management System team");

            Log.Information("Sent notification to: {StudentName}", student.FullName);
            await _emailNotifier.SendEmailAsync(
                student.EmailAddress, "noreply@studentsystem.com", "Course Schedule Reminder", body.ToString());

            await _repo.RemoveEnrollmentsAsync(enrollmentsPerStudent.Select(e => e.EnrollmentId));
        }
    }
}