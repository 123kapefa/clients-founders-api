using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Teledok.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "teledok");

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "teledok",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inn = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "founders",
                schema: "teledok",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inn = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_founders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "legal_entity_founders",
                schema: "teledok",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    founder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_legal_entity_founders", x => x.id);
                    table.ForeignKey(
                        name: "fk_legal_entity_founders_clients_legal_entity_id",
                        column: x => x.legal_entity_id,
                        principalSchema: "teledok",
                        principalTable: "clients",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_legal_entity_founders_founders_founder_id",
                        column: x => x.founder_id,
                        principalSchema: "teledok",
                        principalTable: "founders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clients_inn",
                schema: "teledok",
                table: "clients",
                column: "inn",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_founders_inn",
                schema: "teledok",
                table: "founders",
                column: "inn",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_legal_entity_founders_founder_id",
                schema: "teledok",
                table: "legal_entity_founders",
                column: "founder_id");

            migrationBuilder.CreateIndex(
                name: "ix_legal_entity_founders_legal_entity_id_founder_id",
                schema: "teledok",
                table: "legal_entity_founders",
                columns: new[] { "legal_entity_id", "founder_id" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "legal_entity_founders",
                schema: "teledok");

            migrationBuilder.DropTable(
                name: "clients",
                schema: "teledok");

            migrationBuilder.DropTable(
                name: "founders",
                schema: "teledok");
        }
    }
}
