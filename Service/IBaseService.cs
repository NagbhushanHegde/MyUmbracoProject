namespace MyUmbracoProject.Services
{
    public interface IBaseService
    {
        Task<ServiceResult> ExecuteAsync(Func<Task> action);
    }

    // Reusable result wrapper — use in ALL services
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public static ServiceResult Ok(string message = "Success") =>
            new() { Success = true, Message = message };

        public static ServiceResult Fail(string error) =>
            new() { Success = false, ErrorMessage = error };
    }
}