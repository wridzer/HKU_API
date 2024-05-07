using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HKU_Store.Server.Migrations
{
    /// <inheritdoc />
    public partial class leaderboards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET default_storage_engine=INNODB");

            migrationBuilder.CreateTable(
                name: "AppLeaderboardsInfo",
                columns: table => new
                {
                    ID = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProjectID = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortMethod = table.Column<int>(type: "int", nullable: false),
                    DisplayType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLeaderboardsInfo", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLeaderboardsInfo");
        }
    }
}
