using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LibraryManagementSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEditionWithCopyTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Edition",
                table: "Books");

            migrationBuilder.AddColumn<int>(
                name: "BookCopyId",
                table: "Loans",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookCopies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CopyNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConditionNote = table.Column<string>(type: "text", nullable: true),
                    BookId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCopies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookCopies_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loans_BookCopyId",
                table: "Loans",
                column: "BookCopyId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_BookId",
                table: "BookCopies",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_BookCopies_BookCopyId",
                table: "Loans",
                column: "BookCopyId",
                principalTable: "BookCopies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_BookCopies_BookCopyId",
                table: "Loans");

            migrationBuilder.DropTable(
                name: "BookCopies");

            migrationBuilder.DropIndex(
                name: "IX_Loans_BookCopyId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "BookCopyId",
                table: "Loans");

            migrationBuilder.AddColumn<int>(
                name: "Edition",
                table: "Books",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
