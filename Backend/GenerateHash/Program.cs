// Generar hashes BCrypt reales
string adminPassword = "Admin123!";
string empleadoPassword = "Empleado123!";

string adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, 12);
string empleadoHash = BCrypt.Net.BCrypt.HashPassword(empleadoPassword, 12);

Console.WriteLine("=== HASHES BCRYPT GENERADOS ===");
Console.WriteLine();
Console.WriteLine("USUARIO ADMIN:");
Console.WriteLine($"Username: admin");
Console.WriteLine($"Password: {adminPassword}");
Console.WriteLine($"PasswordHash: {adminHash}");
Console.WriteLine();
Console.WriteLine("USUARIO EMPLEADO:");
Console.WriteLine($"Username: empleado1");
Console.WriteLine($"Password: {empleadoPassword}");
Console.WriteLine($"PasswordHash: {empleadoHash}");
Console.WriteLine();
Console.WriteLine("Copia estos hashes y úsalos en tu INSERT de SQL");
