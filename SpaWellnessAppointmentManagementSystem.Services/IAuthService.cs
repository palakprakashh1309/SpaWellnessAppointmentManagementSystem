using SpaWellnessAppointmentManagementSystem.Repo.Models;
namespace SpaWellnessAppointmentManagementSystem.Services
{
    public interface IAuthService
    {
        User RegisterUser(User user);
        User Login(string username, string password);

        User GetUserById(int userId);
    }
}