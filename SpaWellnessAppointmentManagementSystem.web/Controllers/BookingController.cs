using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using SpaWellnessAppointmentManagementSystem.Repo.Data;
using SpaWellnessAppointmentManagementSystem.Repo.Models;
using SpaWellnessAppointmentManagementSystem.Repo;
using System.Collections.Generic;

namespace SpaWellnessAppointmentManagementSystem.web.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        public IActionResult Details(int id)
        {
            var service = _context.Services.FirstOrDefault(s => s.Id == id);
            if (service == null) return NotFound();

            // Fetch dynamic staff list for the dropdown
            ViewBag.StaffList = (from s in _context.Staffs
                                 join u in _context.Users on s.UserId equals u.UserId
                                 where s.IsActive == true
                                 select new
                                 {
                                     s.StaffId,
                                     s.Specialization,
                                     FullName = u.Username
                                 }).ToList();

            return View(service);
        }

        [HttpPost]
        public IActionResult BookingConfirmation(string serviceName, string category, double price, double tax, double totalPrice, string selectedTime, DateTime appointmentDate, string therapist)
        {
            ViewBag.ServiceName = serviceName;
            ViewBag.Category = category;
            ViewBag.BasePrice = price;
            ViewBag.Tax = tax;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.SelectedTime = selectedTime;
            ViewBag.AppointmentDate = appointmentDate.ToShortDateString();

            // Fix: Ensure the string "Any Available Specialist" is passed if no one is selected
            ViewBag.Therapist = string.IsNullOrEmpty(therapist) ? "Any Available Specialist" : therapist;

            return View("Booking");
        }

        [HttpPost]
        public IActionResult CompleteBooking(string serviceName, string category, string appointmentDate, string selectedTime, string therapist, decimal totalPrice, string paymentMethod, string cardNumber, string upiId, bool isFailed = false)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");

            // Check for "Simulator Failure": If isFailed is true OR if card is all zeros
            bool paymentWasSuccessful = !isFailed;

            // Server-side check for 16 identical digits (e.g., 0000...)
            if (!string.IsNullOrEmpty(cardNumber))
            {
                string rawCard = cardNumber.Replace(" ", "");
                if (rawCard.Length == 16 && rawCard.All(c => c == rawCard[0]))
                {
                    paymentWasSuccessful = false;
                }
            }

            string refNo = "SRN-" + new Random().Next(100000, 999999).ToString();
            string finalTherapist = string.IsNullOrEmpty(therapist) || therapist == "Any Available Specialist" ? "Unassigned" : therapist;

            // Mask card for DB
            string? finalPaymentDetail = null;
            if (paymentMethod == "card" && !string.IsNullOrEmpty(cardNumber))
                finalPaymentDetail = "Card ending in " + (cardNumber.Length > 4 ? cardNumber.Substring(cardNumber.Length - 4) : cardNumber);
            else if (paymentMethod == "upi")
                finalPaymentDetail = upiId;

            var appointment = new Appointment
            {
                CustomerId = int.Parse(userIdStr),
                ServiceName = serviceName,
                Category = category,
                AppointmentDate = DateTime.Parse(appointmentDate),
                SelectedTime = selectedTime,
                Therapist = finalTherapist,
                TotalPrice = totalPrice,
                Status = paymentWasSuccessful ? AppointmentStatus.CONFIRMED : AppointmentStatus.CANCELLED,
                ReferenceNumber = refNo,
                PaymentMethod = paymentMethod,
                PaymentDetail = finalPaymentDetail,
                IsPaymentSuccessful = paymentWasSuccessful // Set based on our check
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            if (!paymentWasSuccessful)
            {
                ViewBag.IsFailed = true;
                ViewBag.ErrorMessage = "The card number provided is invalid or has been declined by the bank.";
                return View("Booking");
            }

            ViewBag.IsSuccess = true;
            ViewBag.ReferenceNumber = refNo;
            return View("Booking");
        }

        [HttpPost]
        public IActionResult CancelAppointment(int appointmentId)
        {
            var appointment = _context.Appointments.Find(appointmentId);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.CANCELLED;
                _context.SaveChanges();
                TempData["BookingSuccess"] = "Appointment " + appointment.ReferenceNumber + " has been cancelled.";
            }
            return RedirectToAction("Customer", "Dashboard");
        }
        // Action to show the print-friendly receipt
        public IActionResult PrintReceipt(string reference)
        {
            // Retrieve the booking from your database using the reference
            var booking = _context.Appointments.FirstOrDefault(a => a.ReferenceNumber == reference);

            if (booking == null) return NotFound();

            return View(booking); // We will create this view next
        }
    }
}