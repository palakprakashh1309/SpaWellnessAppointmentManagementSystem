using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpaWellnessAppointmentManagementSystem.Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentStatusToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentSuccessful",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaymentSuccessful",
                table: "Appointments");
        }
    }
}
