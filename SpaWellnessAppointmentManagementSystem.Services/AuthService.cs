using SpaWellnessAppointmentManagementSystem.Repo.Data;
using SpaWellnessAppointmentManagementSystem.Repo.Models;
using SpaWellnessAppointmentManagementSystem.Services;
using BCrypt.Net; // Add this namespace

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public User RegisterUser(User user)
    {
        // Validation: Check if email or username already exists
        if (_context.Users.Any(u => u.Email == user.Email || u.Username == user.Username))
        {
            return null;
        }

        // --- HASHING IMPLEMENTATION ---
        // Hash the password before saving to the database
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public User Login(string email, string password)
    {
        // 1. Find the user by email first
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        // 2. Use BCrypt.Verify to check if the provided plain-text password 
        // matches the hashed password in the database
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return user; // Success
        }

        return null; // Fail
    }

    public User GetUserById(int userId)
    {
        return _context.Users.FirstOrDefault(u => u.UserId == userId);
    }
}