using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SpaWellnessAppointmentManagementSystem.Services;
using SpaWellnessAppointmentManagementSystem.Repo.Data;
using SpaWellnessAppointmentManagementSystem.Repo.Models;
using SpaWellnessAppointmentManagementSystem.Repo;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SerenitySpa.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public DashboardController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // --- ADMIN DASHBOARD ---
        public IActionResult Admin()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");

            // 1. KPI Calculations
            ViewBag.TotalBookings = _context.Appointments.Count();
            ViewBag.TotalRevenue = _context.Appointments
                .Where(a => a.Status != AppointmentStatus.CANCELLED)
                .Sum(a => (double?)a.TotalPrice) ?? 0.0;
            ViewBag.ActiveCustomers = _context.Appointments.Select(a => a.CustomerId).Distinct().Count();

            // 2. FETCH LATEST APPOINTMENTS
            ViewBag.LatestAppointments = (from a in _context.Appointments
                                          join u in _context.Users on a.CustomerId equals u.UserId
                                          select new
                                          {
                                              ReferenceNumber = a.ReferenceNumber,
                                              CustomerName = u.Username,
                                              ServiceName = a.ServiceName,
                                              RawDate = a.AppointmentDate,
                                              Status = a.Status.ToString()
                                          }).OrderBy(x => x.RawDate).Take(5).ToList();

            // 3. Full Management List
            ViewBag.AllServices = _context.Services.ToList();
            ViewBag.AllAppointments = (from a in _context.Appointments
                                       join u in _context.Users on a.CustomerId equals u.UserId
                                       select new
                                       {
                                           ReferenceNumber = a.ReferenceNumber,
                                           CustomerName = u.Username,
                                           ServiceName = a.ServiceName,
                                           RawDate = a.AppointmentDate,
                                           Time = a.SelectedTime,
                                           Staff = a.Therapist,
                                           Status = a.Status.ToString(),
                                           AppointmentId = a.AppointmentId
                                       }).OrderByDescending(x => x.AppointmentId).ToList();

            // 4. Staff List for Management Cards
            ViewBag.StaffList = (from s in _context.Staffs
                                 join u in _context.Users on s.UserId equals u.UserId
                                 select new
                                 {
                                     s.StaffId,
                                     s.Name,
                                     s.Specialization,
                                     s.IsActive,
                                     Email = u.Email
                                 }).ToList();

            // 5. NEW: User Management List
            ViewBag.UserList = _context.Users
        .Select(u => new {
            // Formats ID to USR-0001 style
            DisplayId = "USR-" + u.UserId.ToString().PadLeft(4, '0'),
            u.UserId,
            u.Username,
            u.Email,
            // Updated to use .Phone
            Phone = u.Phone ?? "(+91) 000-0000",
            Role = u.Role ?? "Customer"
        })
        .OrderByDescending(u => u.UserId)
        .ToList();

            return View();
        }

        // Action to delete a user
        [HttpPost]
        public IActionResult DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                // Safety: Prevent deleting the current logged-in admin
                var currentId = HttpContext.Session.GetString("UserId");
                if (user.UserId.ToString() == currentId)
                {
                    return RedirectToAction("Admin", new { section = "users", error = "self-delete" });
                }

                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Admin", new { section = "users" });
        }

        [HttpPost]
        public IActionResult ToggleStaffStatus(int id)
        {
            var staff = _context.Staffs.Find(id);
            if (staff != null)
            {
                // Flips the current boolean value
                staff.IsActive = !staff.IsActive;
                _context.SaveChanges();
            }
            // Redirects back to the Admin page, specifically to the Staff section
            return RedirectToAction("Admin", new { section = "staff" });
        }

        [HttpGet]
        public IActionResult CreateService()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveService(SpaService newService)
        {
            if (ModelState.IsValid)
            {
                newService.IsAvailable = true;
                _context.Services.Add(newService);
                _context.SaveChanges();
                return RedirectToAction("Admin", new { showServices = true });
            }
            return View("CreateService", newService);
        }

        public IActionResult DeleteService(int id)
        {
            var service = _context.Services.Find(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                _context.SaveChanges();
            }
            return RedirectToAction("Admin", new { showServices = true });
        }

        // --- CUSTOMER DASHBOARD ---
        public IActionResult Customer()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");

            int userId = int.Parse(userIdStr);
            var user = _authService.GetUserById(userId);

            var myBookings = _context.Appointments
                                     .Where(a => a.CustomerId == userId)
                                     .OrderByDescending(a => a.AppointmentDate)
                                     .ToList();

            ViewBag.User = user;
            return View(myBookings); // This passes the List that the view now expects.
        }

        public IActionResult Staff()
        {
            var staffId = HttpContext.Session.GetInt32("StaffId");
            if (staffId == null) return RedirectToAction("Login", "Auth");

            // 1. Get the staff and user data
            var staffData = (from s in _context.Staffs
                             join u in _context.Users on s.UserId equals u.UserId
                             where s.StaffId == staffId
                             select new { Staff = s, UserName = u.Username, DisplayName = s.Name ?? u.Username }).FirstOrDefault();

            if (staffData == null) return Content($"Error: No profile found for StaffId {staffId}.");

            ViewBag.StaffName = staffData.DisplayName;

            // 2. Fetch ALL appointments for this therapist
            var allAppointments = (from a in _context.Appointments
                                   join u in _context.Users on a.CustomerId equals u.UserId
                                   where a.Therapist == staffData.DisplayName
                                   select new
                                   {
                                       AppointmentId = a.AppointmentId,
                                       ServiceName = a.ServiceName,
                                       CustomerName = u.Username,
                                       Time = a.SelectedTime,
                                       Date = a.AppointmentDate,
                                       Status = a.Status.ToString(),
                                       ReferenceNumber = a.ReferenceNumber,
                                       Category = a.Category ?? "Wellness"
                                   }).ToList();

            // 3. Filter for Today (Active) vs History (Completed)
            ViewBag.Appointments = allAppointments.Where(x => x.Status != "COMPLETED").ToList();
            ViewBag.CompletedAppointments = allAppointments.Where(x => x.Status == "COMPLETED").ToList();

            return View(staffData.Staff);
        }
        // --- NEW ACTION: COMPLETE APPOINTMENT ---
        // This is the missing piece to handle the button click on the dashboard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteAppointment(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.COMPLETED;
                _context.SaveChanges();
                TempData["UpdateSuccess"] = "Appointment marked as completed!";
            }
            return RedirectToAction("Staff");
        }

        // --- UPDATED STAFF PROFILE ACTION (SINGLE VERSION) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(int StaffId, string Specialization, string Bio, string ProfilePicture)
        {
            var staff = _context.Staffs.Find(StaffId);

            if (staff != null)
            {
                staff.Specialization = Specialization;
                staff.Bio = Bio;
                staff.ProfilePicture = ProfilePicture;

                _context.SaveChanges();
                TempData["UpdateSuccess"] = "Your professional profile has been updated.";
            }

            return RedirectToAction("Staff");
        }

        [Route("Dashboard/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}