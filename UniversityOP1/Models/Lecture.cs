namespace UniversityOP1.Models
{
    public class Lecture
    {
        public int Id { get; set; }
        public int LectureTheatreId { get; set; }
        public LectureTheatre LectureTheatre { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
