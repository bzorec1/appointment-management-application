using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HairSalonAppointments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiPhaseAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActiveEnd",
                table: "Appointments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActiveStart",
                table: "Appointments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentAppointmentId",
                table: "Appointments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PassiveEnd",
                table: "Appointments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PassiveStart",
                table: "Appointments",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveEnd",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ActiveStart",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ParentAppointmentId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PassiveEnd",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PassiveStart",
                table: "Appointments");
        }
    }
}
