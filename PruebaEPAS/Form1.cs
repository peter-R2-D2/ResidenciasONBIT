﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Specialized;

namespace PruebaEPAS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private SqlConnection conn = new SqlConnection();
        private void Form1_Load(object sender, EventArgs e)
        {
            Llenar_Grid();
        }

        /// <summary>
        /// Verifica que las filas del DataTable vayan en montos incrementales.
        /// </summary>
        /// <param name="dt">DataTable a consultar</param>
        /// <returns>Un booleano indicando true cuando es en incrimento, de lo contrario false.</returns>
        private bool VerificarEnIncremento(DataTable dt, CarroCompra carroCompraEntrante)
        {
            bool resultado;
            float monto;
            monto = dt.Rows[0].Field<float>("MontoTotal");
            resultado = false;
            if (dt != null)
            {
                foreach (DataRow fila in dt.Rows)
                {
                    if (monto <= float.Parse(fila[0].ToString()))
                    {
                        monto = float.Parse(fila[0].ToString());
                        resultado = true;
                    }
                    else
                    {
                        resultado = false;
                        break;
                    }

                }
            }
            return resultado;
        }

        /// <summary>
        /// Verifica que las filas del DataTable vayan en montos en decremento
        /// </summary>
        /// <param name="dt">DataTable a consultar</param>
        /// <returns>Un booleano indicando false cuando es en decremento, de lo contrario true.</returns>
        private bool VerificarEnDecremento(DataTable dt, CarroCompra carroCompraEntrante)
        {
            bool resultado = false;

            try
            {

                bool hayrechazados;
                bool esmontomenor;
                hayrechazados = true;
                esmontomenor = false;
                // Condicion para saber si hayrechazados es igual a falso y
                //si no verificar que los monto de las transacciones son menores que los anteriores.
                if (dt == null)
                {
                    hayrechazados = false;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0].Field<float>("MontoTotal") >= carroCompraEntrante.Carrocomprapago.Montototal)
                        {
                            esmontomenor = true;
                        }
                    }
                    foreach (DataRow fila in dt.Rows)
                    {

                        if (fila.Field<int>("Estatus") != (int)Estatus.Rechazado)
                        {
                            hayrechazados = false;
                        }
                    }
                }

                if (hayrechazados == true && esmontomenor == true)
                {
                    resultado = true;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("No se encontro la variable en el archivo de configuración");

            }
            return resultado;
        }


        private bool VerificarEPA3(DataTable dt, CarroCompra carroCompraEntrante)
        {
            bool resultado = false;
            

            try
            {
                bool intentos = false;

                if (dt == null)
                {
                    intentos = false;
                }
                else
                {
                    foreach (DataRow row in dt.Rows)
                    {

                        if (dt.Rows[0].Field<float>("CodigoRespuesta") != (int)CodigoRespuesta.TarjetaDeclinadaPorCVV)
                        {
                            intentos = false;
                        }
                    }
                }
                resultado = intentos;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);

            }

            return resultado;
        }
        private bool VerificarEPA4(DataTable dt, CarroCompra carroCompraEntrante)
        {
            bool resultado = false;

            try
            {
                bool hayrechazados = true;
                

                if (dt == null)
                {
                    hayrechazados = false;
                }
                else
                {
                    foreach (DataRow fila in dt.Rows)
                    {

                        if (fila.Field<int>("Estatus") != (int)Estatus.Aceptado)
                        {
                            hayrechazados = false;
                        }
                    }
                 }
                return hayrechazados;
            }
            catch (Exception)
            {
                MessageBox.Show("No se encontro la variable en el archivo de configuración");
            }
            return resultado;
        }
        private bool EPA1(CarroCompra carroCompraEntrante)
        {
            bool resultado;
            resultado = false;

            try
            {
                // SP que utilizare para la ejecucion de parametros
                SqlCommand command = new SqlCommand("EPA1", conn);
                command.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(command);

                //Envió los parámetros que necesito para el EPA1
                SqlParameter param1 = new SqlParameter("@userID", SqlDbType.Int);
                param1.Value = carroCompraEntrante.Cliente.Id;
                command.Parameters.Add(param1);

                SqlParameter param2 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param2.Value = carroCompraEntrante.Carrocomprapago.FechaUTCtransaccion;
                command.Parameters.Add(param2);

                SqlParameter param3 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param3.Value = carroCompraEntrante.Carrocomprapago.Tarjetapan;
                command.Parameters.Add(param3);

                String EPA1TiempoDeVentana;
                EPA1TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA1TiempoDeVentana");

                int EPA1NumTransaccionesMin;
                EPA1NumTransaccionesMin = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA1NumTransaccionesMin"));

                SqlParameter param4 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param4.Value = Int32.Parse(EPA1TiempoDeVentana);
                command.Parameters.Add(param4);

                SqlParameter param5 = new SqlParameter("@NumRegistros", SqlDbType.Int);
                param5.Value = EPA1NumTransaccionesMin;
                command.Parameters.Add(param5);

                DataTable dt = new DataTable();

                conn.Open();

                //Aquí ejecuto el SP y lo lleno en el DataTable
                adapter.Fill(dt);
                resultado = VerificarEnIncremento(dt, carroCompraEntrante);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return resultado;
        }

        private bool EPA2(CarroCompra carroCompraEntrante)
        {
            bool resultado;
            resultado = false;


            try
            {
                SqlCommand command = new SqlCommand("EPA2", conn);
                command.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(command);

                //Envió los parámetros que necesito para el EPA2
                SqlParameter param01 = new SqlParameter("@userID", SqlDbType.Int);
                param01.Value = carroCompraEntrante.Cliente.Id;
                command.Parameters.Add(param01);

                SqlParameter param02 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param02.Value = carroCompraEntrante.Carrocomprapago.FechaUTCtransaccion;
                command.Parameters.Add(param02);

                SqlParameter param03 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param03.Value = carroCompraEntrante.Carrocomprapago.Tarjetapan;
                command.Parameters.Add(param03);

                String EPA2TiempoDeVentana;
                EPA2TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA2TiempoDeVentana");

                int EPA2NumTransaccionesMin;
                EPA2NumTransaccionesMin = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA2NumTransaccionesMin"));

                SqlParameter param04 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param04.Value = Int32.Parse(EPA2TiempoDeVentana);
                command.Parameters.Add(param04);

                SqlParameter param05 = new SqlParameter("@NumRegistros", SqlDbType.Int);
                param05.Value = EPA2NumTransaccionesMin;
                command.Parameters.Add(param05);

                DataTable dt = new DataTable();

                conn.Open();

                //Aquí ejecuto el SP y lo lleno en el DataTable
                adapter.Fill(dt);
                resultado = VerificarEnDecremento(dt, carroCompraEntrante);

                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            finally
            {
                conn.Close();
            }
            return resultado;
        }

        private bool EPA3(CarroCompra carroCompraEntrante)
        {
            bool resultado;
            resultado = false;

            try
            {
                SqlCommand command = new SqlCommand("EPA3", conn);
                command.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(command);

                //Envió los parámetros que necesito para el EPA2
                SqlParameter param001 = new SqlParameter("@userID", SqlDbType.Int);
                param001.Value = carroCompraEntrante.Cliente.Id;
                command.Parameters.Add(param001);

                SqlParameter param002 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param002.Value = carroCompraEntrante.Carrocomprapago.FechaUTCtransaccion;
                command.Parameters.Add(param002);

                SqlParameter param003 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param003.Value = carroCompraEntrante.Carrocomprapago.Tarjetapan;
                command.Parameters.Add(param003);

                String EPA3TiempoDeVentana;
                EPA3TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA3TiempoDeVentana");

                int EPA3NumIntentos;
                EPA3NumIntentos = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA3NumIntentos"));

                SqlParameter param004 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param004.Value = Int32.Parse(EPA3TiempoDeVentana);
                command.Parameters.Add(param004);

                SqlParameter param005 = new SqlParameter("@NumIntentos", SqlDbType.Int);
                param005.Value = EPA3NumIntentos;
                command.Parameters.Add(param005);

                DataTable dt = new DataTable();

                conn.Open();

                //Aquí ejecuto el SP y lo lleno en el DataTable
                adapter.Fill(dt);
                resultado = VerificarEPA3(dt, carroCompraEntrante);

                dataGridView1.DataSource = dt;

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                conn.Close();
            }
            return resultado;

        }

        private bool EPA4(CarroCompra carroCompraEntrante)
        {
            bool resultado;
            resultado = false;

            try
            {
                SqlCommand command = new SqlCommand("EPA4", conn);
                command.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter3 = new SqlDataAdapter(command);

                //Envió los parámetros que necesito para el EPA4
                SqlParameter param01 = new SqlParameter("@userID", SqlDbType.Int);
                param01.Value = carroCompraEntrante.Cliente.Id;
                command.Parameters.Add(param01);

                SqlParameter param02 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param02.Value = carroCompraEntrante.Carrocomprapago.FechaUTCtransaccion;
                command.Parameters.Add(param02);

                SqlParameter param03 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param03.Value = carroCompraEntrante.Carrocomprapago.Tarjetapan;
                command.Parameters.Add(param03);

                String EPA4TiempoDeVentana;
                EPA4TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA4TiempoDeVentana");

                int EPA4NumTransaccionesMin;
                EPA4NumTransaccionesMin = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA4NumTransaccionesMin"));

                SqlParameter param04 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param04.Value = Int32.Parse(EPA4TiempoDeVentana);
                command.Parameters.Add(param04);

                SqlParameter param05 = new SqlParameter("@NumRegistros", SqlDbType.Int);
                param05.Value = EPA4NumTransaccionesMin;
                command.Parameters.Add(param05);

                DataTable dt = new DataTable();

                conn.Open();

                //Aquí ejecuto el SP y lo lleno en el DataTable
                adapter3.Fill(dt);
                resultado = VerificarEPA4(dt, carroCompraEntrante);

                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            finally
            {
                conn.Close();
            }
            return resultado;
        }

        private bool EPA5(CarroCompra carroCompraEntrante)
        {
            bool resultado = false;
            try
            {
                SqlCommand command = new SqlCommand("EPA5", conn);
                command.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter4 = new SqlDataAdapter(command);

                //Envió los parámetros que necesito para el EPA4
                SqlParameter param01 = new SqlParameter("@userID", SqlDbType.Int);
                param01.Value = carroCompraEntrante.Cliente.Id;
                command.Parameters.Add(param01);

                SqlParameter param02 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param02.Value = carroCompraEntrante.Carrocomprapago.FechaUTCtransaccion;
                command.Parameters.Add(param02);

                SqlParameter param03 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param03.Value = carroCompraEntrante.Carrocomprapago.Tarjetapan;
                command.Parameters.Add(param03);

                String EPA4TiempoDeVentana;
                EPA4TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA4TiempoDeVentana");

                int EPA4NumTransaccionesMin;
                EPA4NumTransaccionesMin = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA4NumTransaccionesMin"));

                SqlParameter param04 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param04.Value = Int32.Parse(EPA4TiempoDeVentana);
                command.Parameters.Add(param04);

                SqlParameter param05 = new SqlParameter("@NumRegistros", SqlDbType.Int);
                param05.Value = EPA4NumTransaccionesMin;
                command.Parameters.Add(param05);

                DataTable dt = new DataTable();

                conn.Open();

                //Aquí ejecuto el SP y lo lleno en el DataTable
                adapter4.Fill(dt);
                resultado = VerificarEPA4(dt, carroCompraEntrante);

                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            finally
            {
                conn.Close();
            }

            return resultado;
        }
        private void Llenar_Grid()
        {
            // Los argumentos de la conexion de la base de datos
            string args = "Data Source = localhost; Initial Catalog = ProyectoResidencias; Integrated Security = True";
            conn = new SqlConnection();
            conn.ConnectionString = args;

            try
            {
                //Indico el SP EPA1
                SqlCommand command = new SqlCommand("EPA1", conn);
                command.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                //Envió los parámetros que necesitopara el EPA1
                SqlParameter param1 = new SqlParameter("@userID", SqlDbType.Int);
                param1.Value = 00002;
                command.Parameters.Add(param1);

                SqlParameter param2 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param2.Value = DateTimeOffset.Parse("2019-10-16 9:45:00-5");
                command.Parameters.Add(param2);

                SqlParameter param3 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param3.Value = "5474846151371020";
                command.Parameters.Add(param3);
                
                String EPA1TiempoDeVentana;
                EPA1TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA1TiempoDeVentana");

                int EPA1NumTransaccionesMin;
                EPA1NumTransaccionesMin = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA1NumTransaccionesMin"));

                SqlParameter param4 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param4.Value = Int32.Parse(EPA1TiempoDeVentana);
                command.Parameters.Add(param4);

                SqlParameter param5 = new SqlParameter("@NumRegistros", SqlDbType.Int);
                param5.Value = EPA1NumTransaccionesMin;
                command.Parameters.Add(param5);

                //Indico el SP Epa2 
                SqlCommand command1 = new SqlCommand("EPA2", conn);
                command1.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter1 = new SqlDataAdapter(command1);
                //Envió los parámetros que necesito para el EPA2
                SqlParameter param01 = new SqlParameter("@userID", SqlDbType.Int);
                param01.Value = 00002;
                command1.Parameters.Add(param01);

                SqlParameter param02 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param02.Value = DateTimeOffset.Parse("2019-10-21 10:40:00-5");
                command1.Parameters.Add(param02);

                SqlParameter param03 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param03.Value = "5474846151371020";
                command1.Parameters.Add(param03);
                
                String EPA2TiempoDeVentana;
                EPA2TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA2TiempoDeVentana");

                int EPA2NumTransaccionesMin;
                EPA2NumTransaccionesMin = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA2NumTransaccionesMin"));

                SqlParameter param04 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param04.Value = Int32.Parse(EPA2TiempoDeVentana);
                command1.Parameters.Add(param04);

                SqlParameter param05 = new SqlParameter("@NumRegistros", SqlDbType.Int);
                param05.Value = EPA2NumTransaccionesMin;
                command1.Parameters.Add(param05);
                
                //Indico el SP Epa3 
                SqlCommand command2 = new SqlCommand("EPA3", conn);
                command2.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter2 = new SqlDataAdapter(command2);

                //Envió los parámetros que necesito para el EPA3
                SqlParameter param001 = new SqlParameter("@userID", SqlDbType.Int);
                param001.Value = 00002;
                command2.Parameters.Add(param001);

                SqlParameter param002 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param002.Value = DateTimeOffset.Parse("2019-10-21 10:40:00-5");
                command2.Parameters.Add(param002);

                SqlParameter param003 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param003.Value = "5474846151371020";
                command2.Parameters.Add(param003);

                String EPA3TiempoDeVentana;
                EPA3TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA3TiempoDeVentana");

                int EPA3NumIntentos;
                EPA3NumIntentos = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA3NumIntentos"));

                SqlParameter param004 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param004.Value = Int32.Parse(EPA3TiempoDeVentana);
                command2.Parameters.Add(param004);

                SqlParameter param005 = new SqlParameter("@NumIntentos", SqlDbType.Int);
                param005.Value = EPA3NumIntentos;
                command2.Parameters.Add(param005);
             
                //Indico el SP Epa4 
                SqlCommand command3 = new SqlCommand("EPA4", conn);
                command3.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter3 = new SqlDataAdapter(command3);

                //Envió los parámetros que necesito para el EPA4
                SqlParameter param0001 = new SqlParameter("@userID", SqlDbType.Int);
                param0001.Value = 00002;
                command3.Parameters.Add(param0001);

                SqlParameter param0002 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param0002.Value = DateTimeOffset.Parse("2019-10-21 10:40:00-5");
                command3.Parameters.Add(param0002);

                SqlParameter param0003 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param0003.Value = "5474846151371020";
                command3.Parameters.Add(param0003);

                String EPA4TiempoDeVentana;
                EPA4TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA4TiempoDeVentana");

                int EPA4NumTransaccionesMin;
                EPA4NumTransaccionesMin = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA4NumTransaccionesMin"));

                SqlParameter param0004 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param0004.Value = Int32.Parse(EPA4TiempoDeVentana);
                command3.Parameters.Add(param0004);

                SqlParameter param0005 = new SqlParameter("@NumRegistros", SqlDbType.Int);
                param0005.Value = EPA4NumTransaccionesMin;
                command3.Parameters.Add(param0005);

                //Iniciacion del EPA5 

                SqlCommand command4 = new SqlCommand("EPA5", conn);
                command4.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter4 = new SqlDataAdapter(command4);

                //Envió los parámetros que necesito para el EPA5
                SqlParameter param00001 = new SqlParameter("@userID", SqlDbType.Int);
                param00001.Value = 00002;
                command4.Parameters.Add(param00001);

                SqlParameter param00002 = new SqlParameter("FechaTransaccion", SqlDbType.DateTimeOffset);
                param00002.Value = DateTimeOffset.Parse("2019-10-21 10:40:00-5");
                command4.Parameters.Add(param00002);

                SqlParameter param00003 = new SqlParameter("@tarjetaPAN", SqlDbType.VarChar, 19);
                param00003.Value = "5474846151371020";
                command4.Parameters.Add(param00003);

                String EPA5TiempoDeVentana;
                EPA5TiempoDeVentana = ConfigurationManager.AppSettings.Get("EPA5TiempoDeVentana");

                SqlParameter param00004 = new SqlParameter("@Tiempo", SqlDbType.Int);
                param00004.Value = Int32.Parse(EPA5TiempoDeVentana);
                command4.Parameters.Add(param00004);
                
                int EPA5NumTransaccionesMin;
                EPA5NumTransaccionesMin = Int32.Parse(ConfigurationManager.AppSettings.Get("EPA5NumTransaccionesMin"));

                SqlParameter param00005 = new SqlParameter("@NumRegistros", SqlDbType.Int);
                param00005.Value = EPA5NumTransaccionesMin;
                command4.Parameters.Add(param00005);

                DataTable dt = new DataTable();

                conn.Open();

                //Aquí ejecuto el SP y lo lleno en el DataTable
                adapter.Fill(dt);
                adapter1.Fill(dt);
                adapter2.Fill(dt);
                adapter3.Fill(dt);
                adapter4.Fill(dt);

                //Enlazo mis datos obtenidos en el DataTable con el grid
                dataGridView1.DataSource = dt;
            }
            // Excepciones con mensajes para determinar el nivel de error durante el procedimiento
            catch (ArgumentNullException)
            {
                MessageBox.Show("input is null.");
            }
            catch (ArgumentException)
            {
                MessageBox.Show("The offset is greater than 14 hours or less than -14 hours.");
            }

            catch (FormatException)
            {
                MessageBox.Show("input does not contain a valid string representation of a date and time. or" +
                    " input contains the string representation of an offset value without a date or time.");
            }
            catch (OverflowException)
            {
                MessageBox.Show("s represents a number less than MinValue or greater than MaxValue.");
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}