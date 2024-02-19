namespace UniversityOP1.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LecturesPerWeek { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Lecture> Lectures { get; set; }
    }
}
