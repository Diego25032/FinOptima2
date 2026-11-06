using System;
using System.Collections.Generic;

namespace FinOptima.Logica.Validaciones
{
    public class ResultadoValidacion
    {
        private List<string> errores;

        public List<string> Errores
        {
            get { return errores; }
        }

        public bool EsValido
        {
            get { return errores.Count == 0; }
        }

        public string Mensaje
        {
            get
            {
                string texto = "";

                foreach (string error in errores)
                {
                    if (texto.Length > 0)
                    {
                        texto = texto + " ";
                    }

                    texto = texto + error;
                }

                return texto;
            }
        }

        public ResultadoValidacion()
        {
            errores = new List<string>();
        }

        public void AgregarError(string mensaje)
        {
            if (mensaje != null && mensaje.Length > 0)
            {
                errores.Add(mensaje);
            }
        }
    }
}
