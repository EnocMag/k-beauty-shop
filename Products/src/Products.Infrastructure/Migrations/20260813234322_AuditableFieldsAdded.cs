using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AuditableFieldsAdded : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "Products",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Products",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "Products",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "Inventories",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Inventories",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "Inventories",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "Categories",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Categories",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "Categories",
            type: "datetime2",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "Inventories");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Inventories");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "Inventories");

        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "Categories");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Categories");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "Categories");
    }
}
