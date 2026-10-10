using System.Security.Claims;

namespace MoblyUp.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // UID do Firebase do usuário autenticado (claim "sub" do ID token)
        public static string ObterUsuarioId(this ClaimsPrincipal user) =>
            user.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Usuário não autenticado.");

        public static string? ObterEmail(this ClaimsPrincipal user) =>
            user.FindFirstValue("email");
    }
}
