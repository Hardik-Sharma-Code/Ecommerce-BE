using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_BE.Repository.Migrations;

public partial class ChangeProductVendorIdToInt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Drop old FK constraint from Products.VendorId → Users.Id
        migrationBuilder.DropForeignKey(
            name: "FK_Products_Users_VendorId",
            table: "Products");

        // Drop old index if one exists
        migrationBuilder.DropIndex(
            name: "IX_Products_VendorId",
            table: "Products");

        // Add a temporary int column to hold the new VendorProfile.Id
        migrationBuilder.AddColumn<int>(
            name: "VendorId_New",
            table: "Products",
            type: "int",
            nullable: true);

        // Populate from VendorProfiles (match on UserId)
        migrationBuilder.Sql(@"
            UPDATE p
            SET p.VendorId_New = v.Id
            FROM Products p
            INNER JOIN VendorProfiles v ON v.UserId = p.VendorId
        ");

        // Drop the old string column
        migrationBuilder.DropColumn(
            name: "VendorId",
            table: "Products");

        // Rename the new column to VendorId
        migrationBuilder.RenameColumn(
            name: "VendorId_New",
            table: "Products",
            newName: "VendorId");

        // Make it non-nullable (all products must have a vendor)
        migrationBuilder.AlterColumn<int>(
            name: "VendorId",
            table: "Products",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        // Recreate index
        migrationBuilder.CreateIndex(
            name: "IX_Products_VendorId",
            table: "Products",
            column: "VendorId");

        // Add new FK to VendorProfiles
        migrationBuilder.AddForeignKey(
            name: "FK_Products_VendorProfiles_VendorId",
            table: "Products",
            column: "VendorId",
            principalTable: "VendorProfiles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Products_VendorProfiles_VendorId",
            table: "Products");

        migrationBuilder.DropIndex(
            name: "IX_Products_VendorId",
            table: "Products");

        migrationBuilder.AddColumn<string>(
            name: "VendorId_Old",
            table: "Products",
            type: "nvarchar(450)",
            nullable: true);

        migrationBuilder.Sql(@"
            UPDATE p
            SET p.VendorId_Old = v.UserId
            FROM Products p
            INNER JOIN VendorProfiles v ON v.Id = p.VendorId
        ");

        migrationBuilder.DropColumn(
            name: "VendorId",
            table: "Products");

        migrationBuilder.RenameColumn(
            name: "VendorId_Old",
            table: "Products",
            newName: "VendorId");

        migrationBuilder.AlterColumn<string>(
            name: "VendorId",
            table: "Products",
            type: "nvarchar(450)",
            nullable: false,
            defaultValue: string.Empty,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Products_VendorId",
            table: "Products",
            column: "VendorId");

        migrationBuilder.AddForeignKey(
            name: "FK_Products_Users_VendorId",
            table: "Products",
            column: "VendorId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
