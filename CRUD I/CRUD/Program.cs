using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CrudConsola
{
    // modelo de usuario basico
    class User
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public int Edad { get; set; }
        public float Sueldo { get; set; }
        public string Sexo { get; set; } = "";
    }

    // ==================== REPOSITORIO (TXT) ====================
    // - Archivo: usuarios.txt en la carpeta de ejecución
    // - Formato por línea: Id\tNombre\tEmail\tEdad
    // - Primera línea: #nextId=123 (siguiente Id a asignar)
    class UserRepository
    {
        private readonly string _filePath;
        private readonly List<User> _items = new List<User>();
        private int _nextId = 1;

        public UserRepository(string? filePath = null)
        {
            _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "usuarios.txt");
            Cargar();
        }

        public IReadOnlyList<User> Listar() => _items.OrderBy(u => u.Id).ToList();
        public User? Obtener(int id) => _items.FirstOrDefault(u => u.Id == id);

        public User Agregar(string nombre, string email, int edad, float sueldo, string sexo)
        {
            var u = new User { Id = _nextId++, Nombre = nombre, Email = email, Edad = edad, Sueldo = sueldo, Sexo = sexo };
            _items.Add(u);
            Guardar();
            return u;
        }



        // Actualiza campos básicos del usuario. Retorna false si no existe.
        public bool Actualizar(int id, string nombre, string email, int edad, float sueldo, string sexo)
        {
            var u = Obtener(id);
            if (u == null) return false;
            u.Nombre = nombre;
            u.Email = email;
            u.Edad = edad;
            u.Sueldo = sueldo;
            u.Sexo = sexo;
            Guardar();
            return true;
        }

        public bool Eliminar(int id)
        {
            var u = Obtener(id);
            if (u == null) return false;
            _items.Remove(u);
            Guardar();
            return true;
        }

        public void BorrarTodo()
        {
            _items.Clear();
            _nextId = 1;
            Guardar();
        }

        // Guarda en formato tab-delimited con cabecera #nextId
        private void Guardar()
        {
            // Escritura simple 
            using var sw = new StreamWriter(_filePath, false);
            sw.WriteLine($"#nextId={_nextId}");
            foreach (var u in _items.OrderBy(x => x.Id))
            {
                var nom = (u.Nombre ?? "").Replace('\t', ' ').Trim();
                var mail = (u.Email ?? "").Replace('\t', ' ').Trim();
                var sexo = (u.Sexo ?? "").Replace('\t', ' ').Trim();
                sw.WriteLine($"{u.Id}\t{nom}\t{mail}\t{u.Edad}\t{u.Sueldo.ToString(CultureInfo.InvariantCulture)}\t{sexo}");
            }
        }

        // Carga el archivo, ignora líneas inválidas sin romper la ejecución
        private void Cargar()
        {
            if (!File.Exists(_filePath)) return;

            foreach (var linea in File.ReadAllLines(_filePath))
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                if (linea.StartsWith("#nextId=", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(linea.Substring(8), out int n)) _nextId = Math.Max(1, n);
                    continue;
                }

                var p = linea.Split('\t');
                if (p.Length < 4) continue;
                if (!int.TryParse(p[0], out int id)) continue;
                if (!int.TryParse(p[3], out int edad)) continue;

                string sexo = "";
                float sueldo = 0;
                if (p.Length >= 6)
                {
                    float.TryParse(p[4], NumberStyles.Float, CultureInfo.InvariantCulture, out sueldo);
                    sexo = (p[5] ?? "").Replace('\t', ' ').Trim();
                }
                var user = new User
                {
                    Id = id,
                    Nombre = p[1],
                    Email = p[2],
                    Edad = edad,
                    Sueldo = sueldo,
                    Sexo = sexo

                };
                _items.Add(new User{
                    Id = id,
                    Nombre = p[1],
                    Email = p[2],
                    Edad = edad,
                    Sueldo = sueldo,
                    Sexo = sexo
                });
                _nextId = Math.Max(_nextId, id + 1);
            }
        }
    }

    // ==================== APLICACIÓN DE CONSOLA ====================
    class Program
    {
        static UserRepository repo = new UserRepository(RutaCompartida("usuarios.txt"));

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "CRUD de Usuarios - Consola (TXT)";

            while (true)
            {
                MostrarMenu();
                Console.Write("Opción: ");
                var op = Console.ReadLine()?.Trim();

                switch (op)
                {
                    case "1": OpcionAgregar(); break;
                    case "2": OpcionListar(); break;
                    case "3": OpcionBuscar(); break;
                    case "4": OpcionActualizar(); break;
                    case "5": OpcionEliminar(); break;
                    case "6": OpcionBorrarTodo(); break;
                    case "0": Salir(); return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        Pausa();
                        break;
                }
            }
        }

        static void MostrarMenu()
        {
            Console.Clear();
            Console.WriteLine("============== CRUD DE EMPLEADOS ==============");
            Console.WriteLine("1) Agregar");
            Console.WriteLine("2) Listar");
            Console.WriteLine("3) Buscar por Id");
            Console.WriteLine("4) Actualizar");
            Console.WriteLine("5) Eliminar");
            Console.WriteLine("6) Borrar todo");
            Console.WriteLine("0) Salir");
            Console.WriteLine("==================================================");
        }

        // -------- Opciones --------
        static void OpcionAgregar()
        {
            Console.WriteLine("\n-- Agregar usuario --");
            var nombre = LeerNoVacio("Nombre: ");
            var email  = LeerEmail("Email: ");
            var edad = LeerEntero("Edad (0-120): ", 0, 120);
            var sexo= LeerSexo("Sexo (M/F): ");
            var sueldo = LeerSueldoOpcional("Sueldo: ", 0);

            var u = repo.Agregar(nombre, email, edad, sueldo, sexo);
            Console.WriteLine($"\n✔ Agregado con Id {u.Id}");
            Pausa();
        }

        static void OpcionListar()
        {
            Console.WriteLine("\n-- Lista de usuarios --");
            var lista = repo.Listar();
            if (lista.Count == 0)
            {
                Console.WriteLine("(sin registros)");
            }
            else
            {
                ImprimirTabla(lista);
            }
            Pausa();
        }

        static void OpcionBuscar()
        {
            Console.WriteLine("\n-- Buscar por Id --");
            int id = LeerEntero("Id: ", 1, int.MaxValue);
            var u = repo.Obtener(id);
            if (u == null) Console.WriteLine("No se encontró.");
            else ImprimirTabla(new List<User> { u });
            Pausa();
        }

        static void OpcionActualizar()
        {
            Console.WriteLine("\n-- Actualizar usuario --");

            // Mostrar lista antes de pedir el ID (ayuda al usuario)
            var lista = repo.Listar();
            if (lista.Count == 0)
            {
                Console.WriteLine("No hay usuarios para actualizar.");
                Pausa();
                return;
            }
            Console.WriteLine("Usuarios disponibles:");
            ImprimirTabla(lista);

            int id = LeerEntero("Ingresa el Id del usuario que deseas actualizar: ", 1, int.MaxValue);
            var existente = repo.Obtener(id);
            if (existente == null)
            {
                Console.WriteLine("No existe un usuario con ese Id.");
                Pausa();
                return;
            }

            Console.WriteLine($"\nEstás actualizando: {existente.Nombre} (Id {existente.Id})");
            Console.WriteLine("Si dejas un campo vacío, se mantendrá el valor actual.");

            var nombre = LeerOpcional($"Nombre [{existente.Nombre}]: ", existente.Nombre);
            var email  = LeerEmailOpcional($"Email [{existente.Email}]: ", existente.Email);
            var edad = LeerEnteroOpcional($"Edad [{existente.Edad}]: ", existente.Edad, 0, 120);
            var sexo = LeerSexoOpcional($"Sexo [{existente.Sexo}]: ", existente.Sexo);
            var sueldo = LeerSueldoOpcional($"Sueldo [{existente.Sueldo}]: ", existente.Sueldo);

            bool ok = repo.Actualizar(id, nombre, email, edad, sueldo, sexo);
            Console.WriteLine(ok ? "✔ Usuario actualizado exitosamente." : "No se pudo actualizar.");
            Pausa();
        }

        static void OpcionEliminar()
        {
            Console.WriteLine("\n-- Eliminar usuario --");
            int id = LeerEntero("Id a eliminar: ", 1, int.MaxValue);
            var u = repo.Obtener(id);
            if (u == null) { Console.WriteLine("No existe ese Id."); Pausa(); return; }

            Console.Write($"¿Seguro que deseas eliminar a \"{u.Nombre}\"? (s/n): ");
            if ((Console.ReadLine() ?? "").Trim().ToLowerInvariant() == "s")
            {
                bool ok = repo.Eliminar(id);
                Console.WriteLine(ok ? "✔ Eliminado." : "No se pudo eliminar.");
            }
            else Console.WriteLine("Cancelado.");
            Pausa();
        }

        static void OpcionBorrarTodo()
        {
            Console.Write("\nEsto borrará TODOS los datos. ¿Continuar? (s/n): ");
            if ((Console.ReadLine() ?? "").Trim().ToLowerInvariant() == "s")
            {
                repo.BorrarTodo();
                Console.WriteLine("✔ Todo borrado.");
            }
            else Console.WriteLine("Cancelado.");
            Pausa();
        }

        static void Salir()
        {
            Console.WriteLine("\nSaliendo... Los datos ya quedaron guardados en 'usuarios.txt'.");
        }

        // -------- Utilidades de entrada/salida --------

        // Campo obligatorio (no vacío)
        static float LeerSueldoOpcional(string texto, float actual)
        {
            while (true)
            {
                Console.Write(texto);
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) return actual;
                if (float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float n))
                {
                if (n >= 0)
                return n;
                Console.WriteLine("El sueldo no puede ser negativo. Intente de nuevo.");
                }
                else
                {
                Console.WriteLine("Valor inválido. Ingresa un número válido.");
                }
            }
        }

        static string LeerNoVacio(string texto)
        {
            while (true)
            {
                Console.Write(texto);
                var s = (Console.ReadLine() ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(s)) return s;
                Console.WriteLine("Valor obligatorio.");
            }
        }

        // Campo opcional: si se deja vacío, retorna el valor actual
        static string LeerOpcional(string opc, string actual)
        {
            Console.Write(opc);
            var s = Console.ReadLine();
            return string.IsNullOrWhiteSpace(s) ? actual : s!.Trim();
        }

        // Email simple (validación básica)
        static string LeerEmail(string emailV)
        {
            while (true)
            {
                Console.Write(emailV);
                var s = (Console.ReadLine() ?? "").Trim();
                if (EsEmailValido(s)) return s;
                Console.WriteLine("Email inválido.");
            }
        }

        static string LeerEmailOpcional(string opc, string actual)
        {
            while (true)
            {
                Console.Write(opc);
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) return actual;
                var t = s!.Trim();
                if (EsEmailValido(t)) return t;
                Console.WriteLine("Email inválido.");
            }
        }

        // Entero en rango
        static int LeerEntero(string texto, int min, int max)
        {
            while (true)
            {
                Console.Write(texto);
                if (int.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
                    && n >= min && n <= max)
                    return n;
                Console.WriteLine($"Ingresa un entero entre {min} y {max}.");
            }
        }

        // Entero opcional (Enter para mantener)
        static int LeerEnteroOpcional(string texto, int actual, int min, int max)
        {
            while (true)
            {
                Console.Write(texto);
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) return actual;
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) && n >= min && n <= max)
                    return n;
                Console.WriteLine($"Ingresa un entero entre {min} y {max}.");
            }
        }

        static string LeerSexo(string texto)
        {
            while (true)
            {
                Console.Write(texto);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (EsSexoValido(s)) return s;
                Console.WriteLine("Valor inválido. Ingresa M o F.");
            }
        }

        static string LeerSexoOpcional(string texto, string actual)
        {
            while (true)
            {
                Console.Write(texto);
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) return actual;
                var t = s!.Trim().ToUpperInvariant();
                if (EsSexoValido(t)) return t;
                Console.WriteLine("Valor inválido. Ingresa M o F.");
            }
        }
        // Tabla formateada
        static void ImprimirTabla(IReadOnlyList<User> items)
        {
            Console.WriteLine();
            Console.WriteLine($"{Pad("Id", 5)} {Pad("Nombre", 20)} {Pad("Email", 30)} {Pad("Edad", 4)} {Pad("Sexo", 4)} {Pad("Sueldo", 10)}");
            Console.WriteLine(new string('-', 5 + 1 + 20 + 1 + 30 + 1 + 4 + 1 + 4 + 1 + 10));

            foreach (var u in items)
            {
                Console.WriteLine($"{Pad(u.Id.ToString(), 5)} {Pad(u.Nombre, 20)} {Pad(u.Email, 30)} {Pad(u.Edad.ToString(), 4)} {Pad((u.Sexo ?? "").ToUpperInvariant(), 4)} {Pad(u.Sueldo.ToString("F2", CultureInfo.InvariantCulture), 10)}");
            }
            Console.WriteLine();
        }

        static string Pad(string s, int width)
        {
            s ??= "";
            if (s.Length > width) return s.Substring(0, width - 1) + "…";
            return s.PadRight(width);
        }

        static void Pausa()
        {
            Console.Write("\nPresiona ENTER para continuar...");
            Console.ReadLine();
        }

        // Validación básica de email (sin dependencias externas)
        static bool EsEmailValido(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            // Reglas muy básicas: contiene @, un punto después del @, sin espacios
            var at = s.IndexOf('@');
            var dot = s.LastIndexOf('.');
            return at > 0 && dot > at + 1 && dot < s.Length - 1 && !s.Contains(' ');
        }

        static bool EsSexoValido(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim().ToUpperInvariant();
            return s == "M" || s == "F";
        }
        static string RutaCompartida(string nombreArchivo)
        {
            // Se busca la raiz del archivo .txt
            string raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            // asegura que existe la carpeta raíz (ya existe); aquí basta con devolver la ruta del archivo
            return Path.Combine(raiz, nombreArchivo);
        }
    }
}
