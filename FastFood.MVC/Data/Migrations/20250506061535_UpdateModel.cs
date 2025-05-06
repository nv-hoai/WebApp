using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastFood.MVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Promotions_PromotionID1",
                table: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_PromotionID1",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "PromotionID1",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "DiscountedUnitPrice",
                table: "OrderDetails",
                newName: "DiscountedPrice");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "DiscountedPrice",
                table: "OrderDetails",
                newName: "DiscountedUnitPrice");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromotionID1",
                table: "OrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "CartItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    AddressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreetName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressID);
                    table.ForeignKey(
                        name: "FK_Address_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_PromotionID1",
                table: "OrderDetails",
                column: "PromotionID1",
                unique: true,
                filter: "[PromotionID1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Address_CustomerID",
                table: "Address",
                column: "CustomerID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Promotions_PromotionID1",
                table: "OrderDetails",
                column: "PromotionID1",
                principalTable: "Promotions",
                principalColumn: "PromotionID");
        }
    }
}
