using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaWeb.Models
{
    // Contexto específico para a Identity
    public class AppIdentityDbContext : IdentityDbContext<IdentityUser> // Ou um tipo de usuário personalizado se necessário
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options) { }
    }

    // Opcional: Classe de configuração para o contexto principal (separado do Identity)
    // ApplicationDbContext.cs (modifique se existir ou crie)
    // public class ApplicationDbContext : DbContext
    // {
    //     public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    //     // DbSets para seus modelos de domínio (Agendamento, Barbeiro, Servico)
    // }
}