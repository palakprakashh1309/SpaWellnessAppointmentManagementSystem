using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpaWellnessAppointmentManagementSystem.Repo.Data;
using SpaWellnessAppointmentManagementSystem.Repo.Models;
using SpaWellnessAppointmentManagementSystem.Services;
using System.Linq;

namespace SpaWellnessAppointmentManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public AuthController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // POST: /Auth/Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            // Ensure a role is assigned
            if (string.IsNullOrEmpty(user.Role)) user.Role = "CUSTOMER";

            // 1. Save user to the Users table via Service
            var result = _authService.RegisterUser(user);

            if (result != null)
            {
                // 2. Automatically create the Staff Profile entry
                if (user.Role.ToUpper() == "STAFF")
                {
                    // Using the exact class name 'Staffs' as per your Repo file
                    var newStaff = new SpaWellnessAppointmentManagementSystem.Repo.Models.Staffs
                    {
                        UserId = result.UserId,
                        Name = result.Username,
                        Specialization = "Wellness Therapist",
                        Bio = "New staff member profile. Please update your bio.",
                        IsActive = true,
                        ProfilePicture = "default-profile.png" // Added a default to prevent null errors
                    };

                    _context.Staffs.Add(newStaff);
                    _context.SaveChanges();
                }

                return Json(new { success = true, message = "Registration Successful! You can now log in." });
            }

            return Json(new { success = false, message = "Username or Email already exists." });
        }

        // POST: /Auth/Login
        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            var user = _authService.Login(Email, Password);

            if (user != null)
            {
                // Store basic user info in session
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.Username);
                HttpContext.Session.SetString("Role", user.Role.ToUpper());

                if (user.Role.ToUpper() == "STAFF")
                {
                    // This line will no longer crash if you added '?' to your Staff model
                    var staffProfile = _context.Staffs.FirstOrDefault(s => s.UserId == user.UserId);

                    if (staffProfile != null)
                    {
                        HttpContext.Session.SetInt32("StaffId", staffProfile.StaffId);
                        // Use the ?? operator to provide a fallback name if the database value is null
                        HttpContext.Session.SetString("UserName", staffProfile.Name ?? user.Username);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Profile not created. Please contact Admin." });
                    }
                }

                // Determine redirect based on Role
                string redirectUrl = user.Role.ToUpper() switch
                {
                    "ADMIN" => "/Dashboard/Admin",
                    "STAFF" => "/Dashboard/Staff",
                    _ => "/Booking/Index"
                };

                return Json(new { success = true, redirectUrl = redirectUrl });
            }

            return Json(new { success = false, message = "Invalid email or password." });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}