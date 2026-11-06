using System;
using System.Security.Cryptography;

namespace FinOptima.Logica.Seguridad
{
    public static class HashContrasena
    {
        private const int TamanoSal = 16;
        private const int TamanoHash = 32;
        private const int Iteraciones = 100000;

        public static string CrearSal()
        {
            byte[] sal = new byte[TamanoSal];

            using (RandomNumberGenerator generador = RandomNumberGenerator.Create())
            {
                generador.GetBytes(sal);
            }

            return Convert.ToBase64String(sal);
        }

        public static string Calcular(string contrasena, string sal)
        {
            byte[] salEnBytes = Convert.FromBase64String(sal);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, salEnBytes, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
            return Convert.ToBase64String(hash);
        }

        public static bool Verificar(string contrasena, string sal, string hashGuardado)
        {
            if (contrasena == null || sal == null || hashGuardado == null)
            {
                return false;
            }

            string hashCalculado;

            try
            {
                hashCalculado = Calcular(contrasena, sal);
            }
            catch (FormatException)
            {
                return false;
            }

            if (hashCalculado.Length != hashGuardado.Length)
            {
                return false;
            }

            int diferencia = 0;

            for (int posicion = 0; posicion < hashCalculado.Length; posicion = posicion + 1)
            {
                diferencia = diferencia | (hashCalculado[posicion] ^ hashGuardado[posicion]);
            }

            return diferencia == 0;
        }
    }
}
