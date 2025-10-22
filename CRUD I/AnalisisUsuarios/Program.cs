using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AnalisisUsuarios
{
    class User
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public int Edad { get; set; }
        public float Sueldo { get; set; }
        public string Sexo { get; set; } = ""; // M o F
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Ruta compartida en la raíz de la solución
            string filePath = RutaCompartida("usuarios.txt");
            Console.WriteLine($"Usando archivo: {filePath}");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("No se encontró el archivo usuarios.txt.");
                return;
            }

            var usuarios = CargarUsuarios(filePath);

            if (usuarios.Count == 0)
            {
                Console.WriteLine("No hay registros guardados.");
                return;
            }

            // 1. Listar registros
            Console.WriteLine("\n===== LISTA DE USUARIOS =====");
            ImprimirTabla(usuarios);

            // 2. Promedios
            double promedioEdad = usuarios.Average(u => (double)u.Edad);
            double promedioSueldo = usuarios.Average(u => (double)u.Sueldo);

            // 3. Conteo por sexo
            int hombres = usuarios.Count(u => u.Sexo == "M");
            int mujeres = usuarios.Count(u => u.Sexo == "F");
            int sinSexo = usuarios.Count(u => string.IsNullOrWhiteSpace(u.Sexo));

            Console.WriteLine("\n===== ESTADÍSTICAS =====");
            Console.WriteLine($" Total de usuarios: {usuarios.Count}");
            Console.WriteLine($" Promedio de edad: {promedioEdad:F2}");
            Console.WriteLine($" Promedio de sueldo: {promedioSueldo:F2}");
            Console.WriteLine($" Hombres (M): {hombres}");
            Console.WriteLine($" Mujeres (F): {mujeres}");
        }

        // Ruta compartida
        static string RutaCompartida(string nombreArchivo)
        {
            string raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            return Path.Combine(raiz, nombreArchivo);
        }

        // Cargar datos desde archivo
        static List<User> CargarUsuarios(string filePath)
        {
            var lista = new List<User>();

            foreach (var linea in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(linea) || linea.StartsWith("#"))
                    continue;

                var p = linea.Split('\t');
                if (p.Length < 4) continue;

                if (!int.TryParse(p[0], out int id)) continue;
                if (!int.TryParse(p[3], out int edad)) continue;

                string nombre = p[1];
                string email = p[2];
                float sueldo = 0;
                string sexo = "";

                // Formato actual: Id Nombre Email Edad Sueldo Sexo
                if (p.Length >= 6)
                {
                    float.TryParse(p[4], NumberStyles.Float, CultureInfo.InvariantCulture, out sueldo);
                    sexo = (p[5] ?? "").Trim().ToUpperInvariant();
                }
                // Formato anterior: Id Nombre Email Edad Sexo Sueldo
                else if (p.Length == 5)
                {
                    float.TryParse(p[4], NumberStyles.Float, CultureInfo.InvariantCulture, out sueldo);
                }

                lista.Add(new User
                {
                    Id = id,
                    Nombre = nombre,
                    Email = email,
                    Edad = edad,
                    Sueldo = sueldo,
                    Sexo = sexo
                });
            }

            return lista;
        }

        // Mostrar tabla simple
        static void ImprimirTabla(List<User> items)
        {
            Console.WriteLine($"{Pad("Id", 5)} {Pad("Nombre", 20)} {Pad("Email", 25)} {Pad("Edad", 4)} {Pad("Sexo", 4)} {Pad("Sueldo", 10)}");
            Console.WriteLine(new string('-', 5 + 1 + 20 + 1 + 25 + 1 + 4 + 1 + 4 + 1 + 10));

            foreach (var u in items)
            {
                Console.WriteLine($"{Pad(u.Id.ToString(), 5)} {Pad(u.Nombre, 20)} {Pad(u.Email, 25)} {Pad(u.Edad.ToString(), 4)} {Pad(u.Sexo, 4)} {Pad(u.Sueldo.ToString("F2"), 10)}");
            }
        }

        static string Pad(string s, int width)
        {
            if (s == null) s = "";
            return s.Length > width ? s.Substring(0, width - 1) + "…" : s.PadRight(width);
        }
    }
}