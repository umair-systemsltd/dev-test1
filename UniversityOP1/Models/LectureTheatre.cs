namespace UniversityOP1.Models
{
    public class LectureTheatre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public ICollection<Lecture> Lectures { get; set; }
    }
}
