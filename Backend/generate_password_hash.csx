// Script para generar hashes BCrypt de contraseñas
// Para ejecutar: dotnet script generate_password_hash.csx
// Requiere: dotnet tool install -g dotnet-script
// Requiere: dotnet add package BCrypt.Net-Next

#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

Console.WriteLine("=== Generador de Hashes BCrypt ===");
Console.WriteLine();

string password1 = "Admin123!";
string password2 = "Empleado123!";

string hash1 = BCrypt.Net.BCrypt.HashPassword(password1, workFactor: 12);
string hash2 = BCrypt.Net.BCrypt.HashPassword(password2, workFactor: 12);

Console.WriteLine($"Password: {password1}");
Console.WriteLine($"Hash: {hash1}");
Console.WriteLine();

Console.WriteLine($"Password: {password2}");
Console.WriteLine($"Hash: {hash2}");
Console.WriteLine();

// Verificar que los hashes funcionan
bool verify1 = BCrypt.Net.BCrypt.Verify(password1, hash1);
bool verify2 = BCrypt.Net.BCrypt.Verify(password2, hash2);

Console.WriteLine($"Verificación Admin123!: {verify1}");
Console.WriteLine($"Verificación Empleado123!: {verify2}");
