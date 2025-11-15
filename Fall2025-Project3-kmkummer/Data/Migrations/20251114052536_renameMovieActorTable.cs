using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fall2025_Project3_kmkummer.Data.Migrations
{
    /// <inheritdoc />
    public partial class renameMovieActorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.RenameTable(
              name: "CourseStudent",     
             newName: "MovieActors"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
             name: "MovieActors",
             newName: "CourseStudent"
                );
        }
    }
}
