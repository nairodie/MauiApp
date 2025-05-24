using MauiApp2.Interfaces;
using MauiApp2.Models;
using System.Diagnostics;

namespace MauiApp2.Services
{
    public class DummyDataService
    {
        private readonly ITermRepository _termRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly INoteRepository _noteRepository;

        public DummyDataService(ITermRepository termRepository, ICourseRepository courseRepository, IAssessmentRepository assessmentRepository, IInstructorRepository instructorRepository, INoteRepository noteRepository)
        {
            _termRepository = termRepository;
            _courseRepository = courseRepository;
            _assessmentRepository = assessmentRepository;
            _instructorRepository = instructorRepository;
            _noteRepository = noteRepository;
        }

        public async Task<bool> GenerateAllDummyDataAsync()
        {
            try
            {
                await ClearAllDataAsync();

                var results = await Task.WhenAll(
                    GenerateSampleTermsAsync(),
                    GenerateSampleInstructorAsync()
                );

                var coursesResult = await GenerateSampleCoursesAsync();

                var remainingResults = await Task.WhenAll(
                    GenerateSampleAssessmentsAsync(),
                    GenerateSampleNotesAsync()
                );

                return results.Concat(new[] { coursesResult })
                             .Concat(remainingResults)
                             .All(x => x);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating sample data: {ex.Message}");
                return false;
            }
        }

        //TODO: generic base repository DRY
        private async Task ClearAllDataAsync()
        {
            await _noteRepository.DeleteAllAsync();
            await _assessmentRepository.DeleteAllAsync();
            await _courseRepository.DeleteAllAsync();
            await _instructorRepository.DeleteAllAsync();
            await _termRepository.DeleteAllAsync();
        }

        public async Task<bool> GenerateSampleTermsAsync()
        {
            var term1 = new Term("Term 1", DateTime.Now, DateTime.Now.AddMonths(6));
            var term2 = new Term("Term 2", DateTime.Now, DateTime.Now.AddMonths(6));

            await Task.WhenAll(
                _termRepository.InsertAsync(term1),
                _termRepository.InsertAsync(term2)
            );

            Debug.WriteLine("Inserted 2 sample terms");
            return true;
        }

        public async Task<bool> GenerateSampleInstructorAsync()
        {
            var instructor = new Instructor
            {
                Name = "Anika Patel",
                Phone = "555-123-4567",
                Email = "anika.patel@strimeuniversity.edu"
            };

            await _instructorRepository.InsertAsync(instructor);
            Debug.WriteLine("Inserted sample instructor");

            return true;
        }

        public async Task<bool> GenerateSampleCoursesAsync()
        {
            var terms = (await _termRepository.GetAllAsync()).ToList();
            var instructor = (await _instructorRepository.GetAllAsync()).FirstOrDefault();

            if (!terms.Any() || instructor == null)
                return false;

            var now = DateTime.Now;
            var term1 = terms[0];
            var term2 = terms[1];

            var allCourses = new List<Course>();

            var courseTemplates = new List<(string CourseName, Term Term, string Status)>
            {
                ("Math 101", term1, "In Progress"),
                ("English 101", term1, "In Progress"),
                ("Science 101", term1, "In Progress"),
                ("History 101", term1, "In Progress"),
                ("Writing 101", term1, "In Progress"),
                ("Spanish 101", term1, "In Progress"),

                ("Math 202", term2, "Plan to Take"),
                ("English 202", term2, "Plan to Take"),
                ("Science 202", term2, "Plan to Take"),
                ("History 202", term2, "Plan to Take"),
                ("Writing 202", term2, "Plan to Take"),
                ("Spanish 202", term2, "Plan to Take")
            };

            foreach (var (courseName, term, status) in courseTemplates)
            {
                //insert the course 
                var course = new Course
                {
                    TermId = term.TermId,
                    InstructorId = instructor.InstructorId,
                    CourseName = courseName,
                    Start = now,
                    End = now.AddMonths(6),
                    Status = status,
                    CourseDetails = "Enter Course Details Here:"
                };

                //get courseid
                await _courseRepository.InsertAsync(course);

                //create assessments
                var pa = new Assessment
                {
                    CourseId = course.CourseId,
                    Name = "Performance Assessment",
                    Start = now,
                    End = now.AddMonths(6),
                    Details = "Details for performance assessment.",
                    Type = Assessment.AssessmentType.Performance
                };
                await _assessmentRepository.InsertAsync(pa);

                var oa = new Assessment
                {
                    CourseId = course.CourseId,
                    Name = "Objective Assessment",
                    Start = now,
                    End = now.AddMonths(6),
                    Details = "Details for objective assessment.",
                    Type = Assessment.AssessmentType.Objective
                };
                await _assessmentRepository.InsertAsync(oa);

                course.PerformanceAssessmentId = pa.AssessmentId;
                course.ObjectiveAssessmentId = oa.AssessmentId;
                await _courseRepository.UpdateCourseAsync(course);

                allCourses.Add(course);
            }
            Debug.WriteLine($"Inserted {allCourses.Count} sample courses with linked assessments.");
            return false;
        }

        public async Task<bool> GenerateSampleAssessmentsAsync()
        {
            var courses = await _courseRepository.GetAllAsync();
            if (courses.Count == 0) 
                return false;

            var assessments = new List<Assessment>();
            var now = DateTime.Now;

            foreach (var course in courses)
            {
                if (course.PerformanceAssessmentId > 0)
                {
                    var assessment = new Assessment
                    {
                        CourseId = course.CourseId,
                        Name = "Performance Assessment #1",
                        Start = now,
                        End = now.AddMonths(6),
                        Details = "Enter details about assessment here:",
                        Type = Assessment.AssessmentType.Performance
                    };
                    assessments.Add(assessment);
                }

                if (course.ObjectiveAssessmentId > 0)
                {
                    var assessment = new Assessment
                    {
                        CourseId = course.CourseId,
                        Name = "Objective Assessment #1",
                        Start = now,
                        End = now.AddMonths(6),
                        Details = "Enter details about assessment here:",
                        Type = Assessment.AssessmentType.Objective
                    };
                    assessments.Add(assessment);
                }
            }

            var insertTasks = assessments.Select(a => _assessmentRepository.InsertAsync(a));
            await Task.WhenAll(insertTasks);
            Debug.WriteLine($"Inserted {assessments.Count} sample assessments");
            return true;
        }

        public async Task<bool> GenerateSampleNotesAsync()
        {
            var courses = await _courseRepository.GetAllAsync();
            if (courses.Count == 0)
            {
                Debug.WriteLine("No courses found for note assignment.");
                return false;
            }

            var notes = new List<Note>();
            var random = new Random();

            foreach (var course in courses)
            {
                int noteCount = random.Next(1, 3);
                for (int i = 1; i <= noteCount; i++)
                {
                    var note = new Note
                    {
                        CourseId = course.CourseId,
                        Content = $"Sample note {i} for {course.CourseName}"
                    };
                    notes.Add(note);
                }
            }

            var insertTasks = notes.Select(n => _noteRepository.InsertAsync(n));
            await Task.WhenAll(insertTasks);

            Debug.WriteLine($"Inserted {notes.Count} sample notes linked to courses.");
            return true;
        }        
    }
}