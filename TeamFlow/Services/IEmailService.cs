namespace TeamFlow.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }

    public class DummyEmailService : IEmailService
    {
        public Task SendAsync(string to, string subject, string body)
        {
            Console.WriteLine($"[EMAIL] To: {to}\nSubject: {subject}\n{body}");
            return Task.CompletedTask;
        }
    }
}
