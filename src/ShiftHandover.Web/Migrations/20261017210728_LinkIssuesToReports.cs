using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftHandover.Web.Migrations
{
    /// <inheritdoc />
    public partial class LinkIssuesToReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceReportId",
                table: "Issues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_SourceReportId",
                table: "Issues",
                column: "SourceReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_ShiftReports_SourceReportId",
                table: "Issues",
                column: "SourceReportId",
                principalTable: "ShiftReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_ShiftReports_SourceReportId",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_SourceReportId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "SourceReportId",
                table: "Issues");
        }
    }
}
