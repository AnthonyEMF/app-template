namespace API.Services.Seed
{
    public interface ISeedService
    {
        Task LoadRolesAsync();

        Task LoadUsersAsync();
    }
}
