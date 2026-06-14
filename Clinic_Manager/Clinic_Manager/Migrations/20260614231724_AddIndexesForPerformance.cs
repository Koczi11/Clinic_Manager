using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Manager.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Visits_DoctorId_VisitDate",
                table: "Visits",
                columns: new[] { "DoctorId", "VisitDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_DoctorId_VisitDate",
                table: "Visits");
        }
    }
}
