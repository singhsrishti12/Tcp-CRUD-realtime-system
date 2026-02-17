using ExamServer.Data;
using ExamServer.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ExamServer.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace ExamServer.Services
{
    public class TcpCommandProcessor
    {
        private readonly AppDbContext context;

        private readonly WebSocketManager wsManager;

        public TcpCommandProcessor(AppDbContext context, WebSocketManager wsManager)
        {
            this.context = context;
            this.wsManager = wsManager;
        }
        public async Task<string> Process(string command)
        {
            try
            {
                var parts = command.Split('|');
                if (parts.Length != 3)
                    return "ERROR|Invalid command format";

                string operation = parts[0];
                string entity = parts[1];
                string payload = parts[2];

                if (entity == "Student")
                    return await HandleStudent(operation, payload);

                if (entity == "Course")
                    return await HandleCourse(operation, payload);

                return "ERROR|Invalid Entity";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "ERROR|Processing failed";
            }
        }
        async Task<string> HandleStudent(string operation, string payload)
        {
            if (operation == "CREATE")
            {
                var student = JsonSerializer.Deserialize<Student>(payload);
                if (string.IsNullOrEmpty(student.Name))
                    return "ERROR|Name required";
                if (string.IsNullOrEmpty(student.Email))
                    return "ERROR|Email required";
                if(!student.Email.Contains("@"))
                    return "ERROR|Email should contain @";
                context.Students.Add(student);
                await context.SaveChangesAsync();
                await wsManager.BroadCast(
                    JsonSerializer.Serialize(new
                    {
                        eventType = "CREATE",
                        entity = "Student",
                        data = student
                    }));
                 

                return $"OK|Student Created|{JsonSerializer.Serialize(student)}";
            }
            if (operation == "READ")
            {
                var students = context.Students.ToList();

                await wsManager.BroadCast(JsonSerializer.Serialize(new
                {
                    eventType = "READ",
                    entity = "Student",
                    data = students
                }));

                return $"OK|Students List";
            }
            if (operation == "UPDATE")
            {
                var student = JsonSerializer.Deserialize<Student>(payload);
                var existing = await context.Students.FindAsync(student.StudentId);
                if (existing == null)
                {
                    return "ERROR|Student not found";
                }
                if (!string.IsNullOrEmpty(student.Name))
                    existing.Name = student.Name;
                if (!string.IsNullOrEmpty(student.Email) && student.Email.Contains("@"))
                    existing.Email = student.Email;
                await context.SaveChangesAsync();

                await wsManager.BroadCast(
                  JsonSerializer.Serialize(new
                  {
                      eventType = "UPDATE",
                      entity = "Student",
                      data = student
                  }));
                return "OK|Student Updated";
            }
            if (operation == "DELETE")
            {
                var student = JsonSerializer.Deserialize<Student>(payload);
                var existing = await context.Students.FindAsync(student.StudentId);
                if (existing == null)
                {
                    return "ERROR|Student not found";
                }
                context.Students.Remove(existing);
                await context.SaveChangesAsync();
                await wsManager.BroadCast(
                  JsonSerializer.Serialize(new
                  {
                      eventType = "DELETE",
                      entity = "Student",
                      data = student
                  }));
                return "OK|Student Deleted";
            }
            return "ERROR|Invalid operation";
        }
        async Task<string> HandleCourse(string operation, string payload)
        {
            if (operation == "CREATE")
            {
                var course = JsonSerializer.Deserialize<Course>(payload);
                if (string.IsNullOrEmpty(course.Title))
                    return "ERROR|Title required";
                if (course.Credits == 0)
                    return "ERROR|Credits required";
                if(course.Credits<1 || course.Credits>6)
                    return "ERROR|Credits should be between 1 and 6";
                context.Courses.Add(course);
                await context.SaveChangesAsync();
                await wsManager.BroadCast(
                   JsonSerializer.Serialize(new
                   {
                       eventType = "CREATE",
                       entity = "Course",
                       data = course
                   }));

                return $"OK|Course Created|{JsonSerializer.Serialize(course)}";
            }
            if (operation == "READ")
            {
                Console.WriteLine("READ Students broadcasted");

                var courses = context.Courses.ToList();
                await wsManager.BroadCast(JsonSerializer.Serialize(new
                {
                    eventType = "READ",
                    entity = "Course",
                    data = courses
                }));

                return $"OK|Courses List";

            }
            if (operation == "UPDATE")
            {
                var course = JsonSerializer.Deserialize<Course>(payload);
                var existing = await context.Courses.FindAsync(course.CourseId);
                if (existing == null)
                {
                    return "ERROR|Course not found";
                }
                if (!string.IsNullOrEmpty(course.Title))
                    existing.Title = course.Title;
                if (!(course.Credits < 1 || course.Credits > 6))
                    existing.Credits = course.Credits;
                await context.SaveChangesAsync();

                await wsManager.BroadCast(
                  JsonSerializer.Serialize(new
                  {
                      eventType = "UPDATE",
                      entity = "Course",
                      data = course
                  }));
                return "OK|Course Updated";
            }
            if (operation == "DELETE")
            {
                var course = JsonSerializer.Deserialize<Course>(payload);
                var existing = await context.Courses.FindAsync(course.CourseId);
                if (existing == null)
                {
                    return "ERROR|Course not found";
                }
                context.Courses.Remove(existing);
                await context.SaveChangesAsync();
                await wsManager.BroadCast(
                  JsonSerializer.Serialize(new
                  {
                      eventType = "DELETE",
                      entity = "Course",
                      data = course
                  }));
                return "OK|Course Deleted";
            }
            
            return "ERROR|Invalid operation";
        }
    }
}


