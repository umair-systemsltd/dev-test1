using Azure.Core;
using Azure.Core.GeoJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using UniversityOP1.Models;

namespace UniversityOP1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UniversityController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        public UniversityController(UniversityDbContext context)
        {
            _context = context;
        }

        [HttpPost("students")]
        public async Task<IActionResult> CreateStudentAsync(int id, string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Students.Add(
                new Student 
                {
                    Id = id,
                    Name = name
                }
            );
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetStudentById), new { id });
        }

        [HttpGet("students/{studentId}")]
        public ActionResult<Student> GetStudentById(int id)
        {
            var result = _context.Students.Find(id);
            return Ok(result);
        }
        [HttpGet("students")]
        public IActionResult GetAllStudents()
        {
            return Ok(_context.Students);
        }

        

        [HttpPost("enrollments")]
        public async Task<IActionResult> EnrollStudent([FromBody] Enrollment request)
        {
            // Validate the request
            if (request == null)
            {
                return BadRequest("Invalid request");
            }

            // Fetch the subject and student from the database
            var subject = await _context.Subjects
                .Include(s => s.Lectures)
                .FirstOrDefaultAsync(s => s.Id == request.SubjectId);

            var student = await _context.Students
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId);

            if (subject == null || student == null)
            {
                return NotFound("Subject or student not found");
            }

            // Check if student is already enrolled in the subject
            if (student.Enrollments.Any(e => e.SubjectId == request.SubjectId))
            {
                return BadRequest("Student is already enrolled in this subject");
            }

            // Check if the enrollment would exceed lecture theatre capacity
            if (subject.Lectures.Any(l => subject.Enrollments.Count + 1 >  l.LectureTheatre.Capacity))
            {
                return BadRequest("Enrollment would exceed lecture theatre capacity");
            }

            // Check if the enrollment would exceed 10 hours of lectures in a week
            var totalLectureHours = student.Enrollments                
                .Sum(e => e.Subject.Lectures.First().Duration.TotalHours * e.Subject.LecturesPerWeek);

            if (totalLectureHours + subject.Lectures.First().Duration.TotalHours * subject.LecturesPerWeek > 10)
            {
                return BadRequest("Enrollment would exceed 10 hours of lectures in a week");
            }

            // If all checks pass, enroll the student in the subject
            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                SubjectId = subject.Id
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            

            return Ok("Enrollment successful");
        }

        [HttpGet("students/{studentId}/subjects")]
        public async Task<ActionResult<IEnumerable<Subject>>> GetSubjectsForStudentAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null)
            {
                return NotFound("Student Not Found");
            }
            var subjects = student.Enrollments.Select(e => e.Subject).ToList();
            return Ok(subjects);
        }

        [HttpGet("subjects/{subjectId}/students")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudentsForSubject(int subjectId)
        {
            var subject = await _context.Subjects
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == subjectId);
            if (subject == null)
            {
                return NotFound("Subject Not Found");
            }
            var students = subject.Enrollments.Select(e => e.Student).ToList();
            return Ok(students);
        }
    }

}
