using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarimPazari.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioFilePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioFilePath",
                table: "ChatMessages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioFilePath",
                table: "ChatMessages");
        }
    }
}
