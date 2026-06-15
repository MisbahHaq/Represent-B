using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepresentWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSizeToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('ShoppingCarts', 'Size') IS NULL
BEGIN
    ALTER TABLE [ShoppingCarts] ADD [Size] nvarchar(max) NOT NULL DEFAULT N'';
END");

            migrationBuilder.Sql(@"
IF COL_LENGTH('OrderItems', 'Size') IS NULL
BEGIN
    ALTER TABLE [OrderItems] ADD [Size] nvarchar(max) NULL;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('ShoppingCarts', 'Size') IS NOT NULL
BEGIN
    ALTER TABLE [ShoppingCarts] DROP COLUMN [Size];
END");

            migrationBuilder.Sql(@"
IF COL_LENGTH('OrderItems', 'Size') IS NOT NULL
BEGIN
    ALTER TABLE [OrderItems] DROP COLUMN [Size];
END");
        }
    }
}
