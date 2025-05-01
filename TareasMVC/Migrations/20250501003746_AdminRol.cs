using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF NOT EXISTS(SELECT Id FROM AspNetRoles WHERE Id = '03ae2450-6567-4b56-9b0f-6b3878ac5709')
                                    BEGIN
                                    INSERT INTO AspNetRoles(Id,Name,NormalizedName)
                                    VALUES ('03ae2450-6567-4b56-9b0f-6b3878ac5709','admin','ADMIN');
                                    END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE AspNetRoles WHERE Id = '03ae2450-6567-4b56-9b0f-6b3878ac5709'");
        }
    }
}
