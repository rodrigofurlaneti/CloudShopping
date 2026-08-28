using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Services
{
    public sealed class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            // Utiliza BCrypt para gerar um hash seguro com salt automático
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hashedPassword)
        {
            // Verifica se a senha bate com o hash armazenado no banco
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
