namespace Common.Model
{
    public class Advisor
    {
        public string Name { get; set; }
        public string? Email { get; set; }

        public Advisor(string name, string? email = null)
        {
            Name = name;
            Email = email;
        }
    }
}