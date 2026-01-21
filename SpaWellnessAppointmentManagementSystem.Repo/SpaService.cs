using System.ComponentModel.DataAnnotations;

namespace SpaWellnessAppointmentManagementSystem.Repo.Models
{
    public class SpaService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Icon { get; set; } // e.g., "💆"

        [Required]
        public string Category { get; set; }

        [Required]
        public double Price { get; set; }

        public string? Duration { get; set; }

        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;
        public string? Gradient { get; set; }
        public string? WhatToExpect { get; set; }
    }
}