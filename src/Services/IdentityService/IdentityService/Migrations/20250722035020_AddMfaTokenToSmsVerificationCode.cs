using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Migrations
{
    /// <inheritdoc />
    public partial class AddMfaTokenToSmsVerificationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MfaToken",
                table: "SmsVerificationCodes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsVerificationCodes_MfaToken",
                table: "SmsVerificationCodes",
                column: "MfaToken",
                unique: true,
                filter: "[MfaToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SmsVerificationCodes_MfaToken",
                table: "SmsVerificationCodes");

            migrationBuilder.DropColumn(
                name: "MfaToken",
                table: "SmsVerificationCodes");
        }
    }
}
