using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Ship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ship",
                columns: table => new
                {
                    ship_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ship_name = table.Column<string>(type: "TEXT", nullable: false),
                    ship_name_suffix = table.Column<string>(type: "TEXT", nullable: false),
                    file_path = table.Column<string>(type: "TEXT", nullable: false),
                    fallback_file_path = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ship", x => x.ship_id);
                });

            migrationBuilder.CreateTable(
                name: "profile_ship",
                columns: table => new
                {
                    profiles_id = table.Column<int>(type: "INTEGER", nullable: false),
                    ships_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profile_ship", x => new { x.profiles_id, x.ships_id });
                    table.ForeignKey(
                        name: "FK_profile_ship_profile_profiles_id",
                        column: x => x.profiles_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_profile_ship_ship_ships_id",
                        column: x => x.ships_id,
                        principalTable: "ship",
                        principalColumn: "ship_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_profile_ship_ships_id",
                table: "profile_ship",
                column: "ships_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profile_ship");

            migrationBuilder.DropTable(
                name: "ship");
        }
    }
}
