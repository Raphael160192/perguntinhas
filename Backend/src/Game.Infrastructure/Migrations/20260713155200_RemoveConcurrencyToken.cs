using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConcurrencyToken : Migration
    {
        // O xmin é coluna de SISTEMA do PostgreSQL: nunca foi criada por migration e
        // não pode ser dropada. Esta migration existe apenas para atualizar o snapshot
        // do modelo (remoção do token de concorrência); o schema físico não muda.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
