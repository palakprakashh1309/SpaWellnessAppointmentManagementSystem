using SpaWellnessAppointmentManagementSystem.Repo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SpaWellnessAppointmentManagementSystem.Repo
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        // Matches 'serviceName' from BookingController 
        [Required]
        [StringLength(100)]
        public required string ServiceName { get; set; }

        // Matches 'category' from BookingController 
        [StringLength(50)]
        public required string Category { get; set; }

        // Matches 'appointmentDate' from BookingController 
        [Required]
        [DataType(DataType.Date)]
        public required DateTime AppointmentDate { get; set; }

        // Matches 'selectedTime' used in the view
        [Required]
        [StringLength(20)]
        public required string SelectedTime { get; set; }

        // Matches 'therapist' from BookingController 
        [StringLength(100)]
        public required string Therapist { get; set; }

        // Matches 'totalPrice' from BookingController 
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } 

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.CONFIRMED;

        // Reference ID generated in the success screen (e.g., SRN-123456) 
        [StringLength(20)]
        public required string ReferenceNumber { get; set; }

        // In Appointment.cs
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? PaymentDetail { get; set; }

        public bool IsPaymentSuccessful { get; set; } = false;

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual User User { get; set; }
    }

    public enum AppointmentStatus
    {
        CONFIRMED,
        CANCELLED,
        COMPLETED
    }

}
