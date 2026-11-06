using System;
using System.Collections.Generic;
using FinOptima.Logica.Modelos;

namespace FinOptima.Logica.Servicios
{
    public class ServicioOptimizacion
    {
        public ResultadoOptimizacion Optimizar(decimal ingresoMensual, List<RestriccionSobre> restricciones)
        {
            ResultadoOptimizacion resultado = new ResultadoOptimizacion();
            resultado.IngresoMensual = ingresoMensual;

            if (ingresoMensual < 0)
            {
                resultado.Mensaje = "El ingreso mensual no puede ser negativo.";
                return resultado;
            }

            if (restricciones == null || restricciones.Count == 0)
            {
                resultado.Mensaje = "Agrega al menos un sobre para calcular la distribucion.";
                return resultado;
            }

            decimal sumaMinimos = 0;
            decimal sumaMaximos = 0;

            foreach (RestriccionSobre restriccion in restricciones)
            {
                if (restriccion.Minimo < 0 || restriccion.Maximo > 100 || restriccion.Minimo > restriccion.Maximo)
                {
                    resultado.Mensaje = "Las restricciones del sobre \"" + restriccion.Nombre + "\" no son coherentes.";
                    return resultado;
                }

                if (restriccion.Objetivo < restriccion.Minimo || restriccion.Objetivo > restriccion.Maximo)
                {
                    resultado.Mensaje = "El objetivo del sobre \"" + restriccion.Nombre + "\" debe estar entre su minimo y su maximo.";
                    return resultado;
                }

                sumaMinimos = sumaMinimos + restriccion.Minimo;
                sumaMaximos = sumaMaximos + restriccion.Maximo;
            }

            if (sumaMinimos > 100)
            {
                resultado.Mensaje = "No es posible repartir el 100%: los minimos de los sobres suman " + sumaMinimos.ToString("0.##") + "%.";
                return resultado;
            }

            if (sumaMaximos < 100)
            {
                resultado.Mensaje = "No es posible repartir el 100%: los maximos de los sobres suman " + sumaMaximos.ToString("0.##") + "%.";
                return resultado;
            }

            decimal[] valores = new decimal[restricciones.Count];

            for (int indice = 0; indice < restricciones.Count; indice = indice + 1)
            {
                valores[indice] = restricciones[indice].Objetivo;

                if (valores[indice] < restricciones[indice].Minimo)
                {
                    valores[indice] = restricciones[indice].Minimo;
                }

                if (valores[indice] > restricciones[indice].Maximo)
                {
                    valores[indice] = restricciones[indice].Maximo;
                }
            }

            decimal suma = 0;

            for (int indice = 0; indice < valores.Length; indice = indice + 1)
            {
                suma = suma + valores[indice];
            }

            if (suma < 100)
            {
                bool alcanza = Aumentar(valores, restricciones, 100 - suma);

                if (!alcanza)
                {
                    resultado.Mensaje = "No hay capacidad suficiente para llegar al 100%.";
                    return resultado;
                }
            }
            else if (suma > 100)
            {
                bool alcanza = Reducir(valores, restricciones, suma - 100);

                if (!alcanza)
                {
                    resultado.Mensaje = "No se puede reducir lo suficiente para llegar al 100%.";
                    return resultado;
                }
            }

            AjustarRedondeo(valores, restricciones);

            for (int indice = 0; indice < valores.Length; indice = indice + 1)
            {
                AsignacionSobre asignacion = new AsignacionSobre();
                asignacion.SobreId = restricciones[indice].Id;
                asignacion.Nombre = restricciones[indice].Nombre;
                asignacion.Porcentaje = valores[indice];
                asignacion.Monto = ingresoMensual * valores[indice] / 100;
                resultado.Asignaciones.Add(asignacion);
                resultado.TotalAsignado = resultado.TotalAsignado + asignacion.Monto;
            }

            resultado.EsFactible = true;
            resultado.Mensaje = "Distribucion calculada correctamente.";
            return resultado;
        }

        private static bool Aumentar(decimal[] valores, List<RestriccionSobre> restricciones, decimal cantidad)
        {
            decimal capacidad = 0;

            for (int indice = 0; indice < valores.Length; indice = indice + 1)
            {
                capacidad = capacidad + restricciones[indice].Maximo - valores[indice];
            }

            if (capacidad + 0.0001m < cantidad)
            {
                return false;
            }

            if (capacidad <= 0)
            {
                return false;
            }

            for (int indice = 0; indice < valores.Length; indice = indice + 1)
            {
                decimal espacio = restricciones[indice].Maximo - valores[indice];

                if (espacio > 0)
                {
                    valores[indice] = valores[indice] + cantidad * espacio / capacidad;
                }
            }

            return true;
        }

        private static bool Reducir(decimal[] valores, List<RestriccionSobre> restricciones, decimal cantidad)
        {
            decimal disponible = 0;

            for (int indice = 0; indice < valores.Length; indice = indice + 1)
            {
                disponible = disponible + valores[indice] - restricciones[indice].Minimo;
            }

            if (disponible + 0.0001m < cantidad)
            {
                return false;
            }

            if (disponible <= 0)
            {
                return false;
            }

            for (int indice = 0; indice < valores.Length; indice = indice + 1)
            {
                decimal margen = valores[indice] - restricciones[indice].Minimo;

                if (margen > 0)
                {
                    valores[indice] = valores[indice] - cantidad * margen / disponible;
                }
            }

            return true;
        }

        private static void AjustarRedondeo(decimal[] valores, List<RestriccionSobre> restricciones)
        {
            decimal suma = 0;

            for (int indice = 0; indice < valores.Length; indice = indice + 1)
            {
                valores[indice] = Math.Round(valores[indice], 2);
                suma = suma + valores[indice];
            }

            decimal diferencia = 100 - suma;
            int intentos = 0;

            while (diferencia != 0 && intentos < 1000)
            {
                bool cambio = false;

                for (int indice = 0; indice < valores.Length; indice = indice + 1)
                {
                    if (diferencia > 0 && valores[indice] + 0.01m <= restricciones[indice].Maximo)
                    {
                        valores[indice] = valores[indice] + 0.01m;
                        diferencia = diferencia - 0.01m;
                        cambio = true;
                        break;
                    }

                    if (diferencia < 0 && valores[indice] - 0.01m >= restricciones[indice].Minimo)
                    {
                        valores[indice] = valores[indice] - 0.01m;
                        diferencia = diferencia + 0.01m;
                        cambio = true;
                        break;
                    }
                }

                if (!cambio)
                {
                    break;
                }

                intentos = intentos + 1;
            }
        }
    }
}
