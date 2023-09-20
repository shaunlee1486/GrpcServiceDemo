namespace GrpcServiceDemo.Model
{
    public class TodoItem
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string TodoStatus { get; set; } = "NEW";
    }
}
