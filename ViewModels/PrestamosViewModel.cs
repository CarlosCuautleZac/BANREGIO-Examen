using BANREGIO.Models;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BANREGIO.ViewModels
{
    public class PrestamosViewModel : INotifyPropertyChanged
    {
        //Propiedades de la ventana
        public string Nombre { get; set; } = "";
        public string RFC { get; set; } = "";
        public DateTime FechaNacimiento { get; set; } = DateTime.Now.AddYears(-18);
        public decimal ImporteASolicitar { get; set; }
        public decimal IngresosMensuales { get; set; }
        public string Resultado { get; set; } = "";

        public List<Prestamo>? Prestamos { get; set; } = new();

        //Comando para ejectuar la consulta del cliente
        public ICommand CalcularCommand { get; set; }
        public ICommand GuardarDatosCommand { get; set; }
        public PrestamosViewModel()
        {
            CalcularCommand = new RelayCommand(Calcular);
            GuardarDatosCommand = new RelayCommand(GuardarDatos);
            VerificarDatos();
        }

        private void GuardarDatos()
        {
            //Serializamos
            string json = JsonConvert.SerializeObject(Prestamos);
            //Guardamos los datos
            File.WriteAllText("datos.json", json);
        }

        private void VerificarDatos()
        {
            if (!File.Exists("datos.json"))
            { 
                Prestamo p1 = new()
                {
                    ClienteID = 1,
                    Nombre = "Juan",
                    RFC = "GOTJ950101",
                    Fecha = new DateTime(2020,08,25),
                    Importe = 18500,
                    Aprobado = true
                };

                Prestamo p2 = new()
                {
                    ClienteID = 2,
                    Nombre = "María",
                    RFC = "LOVM901015",              
                    Fecha = new DateTime(2021, 04, 15),
                    Importe = 25000,
                    Aprobado = false
                };

                Prestamo p3 = new()
                {
                    ClienteID = 3,
                    Nombre = "Andrés",
                    RFC = "SIMA961201",
                    Fecha = new DateTime(2019, 02, 12),
                    Importe = 13800,
                    Aprobado = true
                };

                

                Prestamo p4 = new()
                {
                    ClienteID = 4,
                    Nombre = "Sofia",
                    RFC = "ZARS861218",
                    Fecha = new DateTime(2018, 05, 15),
                    Importe = 40000,
                    Aprobado = false
                };

                //Agregamos datos al banco de datos
                Prestamos.Add(p1);
                Prestamos.Add(p2);
                Prestamos.Add(p3);
                Prestamos.Add(p4);

                GuardarDatos();
            }
            else
            {
                string json = File.ReadAllText("datos.json");
                Prestamos = JsonConvert.DeserializeObject<List<Prestamo>>(json);
            }
        }

        private void Calcular()
        {
            Resultado = "";

            //VALIDACIONES DE ENTRADA DE DATOS
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                Resultado += "El campo nombre no debe ir vacio"+Environment.NewLine;
            }
            if (string.IsNullOrWhiteSpace(RFC))
            {
                Resultado += "El campo RFC no debe ir vacio" + Environment.NewLine;
            }

            //Si se quisiera validar un rfc real
            //else
            //{
            //    Resultado += ValidarRFC(RFC);
            //}

            if (ImporteASolicitar <= 0)
            {
                Resultado += "El importe a solicitar no debe ser menor a 0" + Environment.NewLine;

            }
            if (IngresosMensuales <= 0)
            {
                Resultado += "Los ingresos mensuales no deben ser menores a 0" + Environment.NewLine;

            }

            if (FechaNacimiento >= DateTime.Now)
            {
                Resultado += "Fecha de nacimiento incorrecta" + Environment.NewLine;
            }

            if (!EsMayorDeEdad(FechaNacimiento))
            {
                Resultado += "Rechazado, Menor de edad" + Environment.NewLine;

                //var id = Prestamos.Max(x => x.ClienteID) + 1;

                //Prestamo prestamo = new()
                //{
                //    ClienteID = id,
                //    Nombre = Nombre,
                //    Fecha = DateTime.Now,
                //    Importe = ImporteASolicitar,
                //    RFC = RFC,
                //    Aprobado = false,
                //    Razon = "La solicitud ha sido rechazada. El cliente es menor de edad."
                //};

                //Prestamos.Add(prestamo);

                AddList(false, ImporteASolicitar, "La solicitud ha sido rechazada. El cliente es menor de edad.");
            }

            //Em caso de no cumplir con los ingresos mensuales minimos se rechaza
            if (IngresosMensuales < 5000)
            {
                Resultado += "Rechazado. No cumple con los ingresos mensuales minimos" + Environment.NewLine;

                //var id = Prestamos.Max(x => x.ClienteID) + 1;

                //Prestamo prestamo = new()
                //{
                //    ClienteID = id,
                //    Nombre = Nombre,
                //    Fecha = DateTime.Now,
                //    Importe = ImporteASolicitar,
                //    RFC = RFC,
                //    Aprobado = false,
                //    Razon = "Rechazado. No cumple con los ingresos mensuales minimos."
                //};

                //Prestamos.Add(prestamo);

                
                AddList(false, ImporteASolicitar, "Rechazado. No cumple con los ingresos mensuales minimos.");
            }

            //Si resultado esta vacio es que no tiene errores
            if(Resultado == "")
            {
                if (IngresosMensuales>=5000 && IngresosMensuales < 9999.99m)
                {
                    Caso1();
                }
                else if (IngresosMensuales >= 10000 && IngresosMensuales < 19999.99m)
                {
                    Caso2();
                }
                else if (IngresosMensuales >= 20000 && IngresosMensuales < 39999.99m)
                {
                    Caso3();
                }
                //40 mil o mas
                else
                {
                    Caso4();
                }
            }

            OnPropertyChanged();

        }

        private void Caso4()
        {
            var tienehistorial = Prestamos.Any(x => x.RFC == RFC && x.Aprobado == true && x.Fecha >= DateTime.Now.AddYears(-2));

            if (tienehistorial)
            {
                if (ImporteASolicitar <= 100000)
                {
                    Resultado = $"La solicitud ha sido aprobada. Se aprueba un monto de {ImporteASolicitar.ToString("c")} " +
                        $"al cliente";

                    AddList(true, ImporteASolicitar, Resultado);
                }
                else
                {
                    Resultado = $"La solicitud ha sido rechazada. Aunque se tenga historial crediticio" +
                        $"solo se podria aprobar un monto maximo de $100,000.00 ";


                    AddList(false, ImporteASolicitar, Resultado);
                }

            }
            else
            {
                if (ImporteASolicitar <= 50000)
                {
                    Resultado = $"La solicitud ha sido aprobada. Se aprueba un monto de {ImporteASolicitar.ToString("c")} " +
                        $"al cliente";

                    AddList(true, ImporteASolicitar, Resultado);
                }
                else
                {
                    Resultado = $"La solicitud ha sido rechazada.No tiene historial crediticio y " +
                        $"solo se podria aprobar un monto maximo de $50,000.00 ";


                    AddList(false, ImporteASolicitar, Resultado);
                }
            }
        }

        private void Caso3()
        {
            var tienehistorial = Prestamos.Any(x => x.RFC == RFC && x.Aprobado == true && x.Fecha >= DateTime.Now.AddYears(-2));

            if (tienehistorial)
            {
                if (ImporteASolicitar <= 50000)
                {
                    Resultado = $"La solicitud ha sido aprobada. Se aprueba un monto de {ImporteASolicitar.ToString("c")} " +
                        $"al cliente";

                    AddList(true, ImporteASolicitar, Resultado);
                }
                else
                {
                    Resultado = $"La solicitud ha sido rechazada. Aunque se tenga historial crediticio" +
                        $"solo se podria aprobar un monto maximo de $50,000.00 ";


                    AddList(false, ImporteASolicitar, Resultado);
                }

            }
            else
            {
                if (ImporteASolicitar <= 30000)
                {
                    Resultado = $"La solicitud ha sido aprobada. Se aprueba un monto de {ImporteASolicitar.ToString("c")} " +
                        $"al cliente";

                    AddList(true, ImporteASolicitar, Resultado);
                }
                else
                {
                    Resultado = $"La solicitud ha sido rechazada.No tiene historial crediticio y " +
                        $"solo se podria aprobar un monto maximo de $30,000.00 ";


                    AddList(false, ImporteASolicitar, Resultado);
                }
            }
        }

        private void Caso2()
        {
            var tienehistorial = Prestamos.Any(x => x.RFC == RFC && x.Aprobado == true && x.Fecha >= DateTime.Now.AddYears(-2));

            if (tienehistorial)
            {
                if (ImporteASolicitar <= 25000)
                {
                    Resultado = $"La solicitud ha sido aprobada. Se aprueba un monto de {ImporteASolicitar.ToString("c")} " +
                        $"al cliente";

                    AddList(true, ImporteASolicitar, Resultado);
                }
                else
                {
                    Resultado = $"La solicitud ha sido rechazada. Aunque se tenga historial crediticio" +
                        $"solo se podria aprobar un monto maximo de $25,000.00 ";


                    AddList(false, ImporteASolicitar, Resultado);
                }

            }
            else
            {
                if (ImporteASolicitar <= 12000)
                {
                    Resultado = $"La solicitud ha sido aprobada. Se aprueba un monto de {ImporteASolicitar.ToString("c")} " +
                        $"al cliente";

                    AddList(true, ImporteASolicitar, Resultado);
                }
                else
                {
                    Resultado = $"La solicitud ha sido rechazada.No tiene historial crediticio y " +
                        $"solo se podria aprobar un monto maximo de $12,000.00 ";


                    AddList(false, ImporteASolicitar, Resultado);
                }
            }
        }

        private void Caso1()
        {
            var tienehistorial = Prestamos.Any(x=>x.RFC == RFC && x.Aprobado == true && x.Fecha >= DateTime.Now.AddYears(-2));

            if(tienehistorial)
            {
                if (ImporteASolicitar <= 15000)
                {
                    Resultado = $"La solicitud ha sido aprobada. Se aprueba un monto de {ImporteASolicitar.ToString("c")} " +
                        $"al cliente";

                    AddList(true, ImporteASolicitar, Resultado);
                }
                else
                {
                    Resultado = $"La solicitud ha sido rechazada. Aunque se tenga historial crediticio" +
                        $"solo se podria aprobar un monto maximo de $15,000.00 ";

                   
                    AddList(false, ImporteASolicitar, Resultado);
                }
                    
            }
            else
            {
                if (ImporteASolicitar <= 7500)
                {
                    Resultado = $"La solicitud ha sido aprobada. Se aprueba un monto de {ImporteASolicitar.ToString("c")} " +
                        $"al cliente";

                    AddList(true, ImporteASolicitar, Resultado);
                }
                else
                {
                    Resultado = $"La solicitud ha sido rechazada.No tiene historial crediticio y " +
                        $"solo se podria aprobar un monto maximo de $7,500.00 ";


                    AddList(false, ImporteASolicitar, Resultado);
                }
            }
        }

        private void AddList(bool aprobado, decimal importe, string razon)
        {
            var id = Prestamos.Max(x => x.ClienteID) + 1;
            Prestamo prestamo = new()
            {
                ClienteID = id,
                Nombre = Nombre,
                Fecha = DateTime.Now,
                Importe = importe,
                RFC = RFC,
                Aprobado = aprobado,
                Razon = razon
            };

            Prestamos.Add(prestamo);
            OnPropertyChanged();
        }

        private string ValidarRFC(string rfc)
        {
            if (Regex.IsMatch(rfc, "[A-z]{4}[0-9]{6}[A-z0-9]{3}") || Regex.IsMatch(rfc, "[A-z]{3}[0-9]{6}[A-z0-9]{3}"))
            {
                return "";
            }
            else
            {
                return $"El {rfc} no es valido" + Environment.NewLine;
            }
        }

        bool EsMayorDeEdad(DateTime fechaNacimiento)
        {
            DateTime fechaActual = DateTime.Today;
            int edad = fechaActual.Year - fechaNacimiento.Year;

            if (fechaNacimiento > fechaActual.AddYears(-edad))
            {
                edad--;
            }

            return edad >= 18;
        }

        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));  
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
