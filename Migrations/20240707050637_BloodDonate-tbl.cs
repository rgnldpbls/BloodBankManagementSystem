using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMS.Migrations
{
    /// <inheritdoc />
    public partial class BloodDonatetbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BloodDonate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Age = table.Column<int>(type: "int", nullable: false),
                    BloodType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitNo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonateDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DonatePlace = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodDonate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodDonate_Donor_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Donor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloodDonate_DonorId",
                table: "BloodDonate",
                column: "DonorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloodDonate");
        }
    }
}
