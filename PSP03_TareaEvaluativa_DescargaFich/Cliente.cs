using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PSP03_Socket_TCP
{
    internal class Cliente
    {
        public static int Main(String[] args)
        {

            Cliente cliente = new Cliente();
            cliente.FuncionServidor();

            Console.WriteLine("Pulse intro para continuar");
            Console.ReadLine();

            return 0;
        }
        private void FuncionServidor()
        {
            Socket sender = null;
            try
            {
                int port = 12000;
                string data = null;


                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[1];

                sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("Programa cliente iniciando.\n");

                //Conexión de socket al servidor
                IPEndPoint iPEndPoint = new IPEndPoint(ipAddress.Address, port); //Indicamos IP de servidor y puerto del servidor
                sender.Connect(iPEndPoint); //Se establece la conexión
                Console.WriteLine("Socket conectado a servidor {0}\n", sender.RemoteEndPoint.ToString()); //Mostramos por pantalla que todo ha ido correcto



                //Recepción de información
                Console.WriteLine("Cliente preparado para la transferencia de datos con servidor.\n");

                //Menu
                bool salir = false;

                while (!salir)
                {

                    try
                    {
                        Console.WriteLine("Elija una de las siguientes imágenes para su descarga:\n" +
                            "*****************************************************\n");
                        Console.WriteLine("1. FotoMonte");
                        Console.WriteLine("2. FotoPlaya");
                        Console.WriteLine("3. FotoCiudad");
                        Console.WriteLine("4. Salir");
                        Console.WriteLine("Elige una de las opciones:");

                        data = string.Empty;
                        data = Console.ReadLine();

                        int opcion = Convert.ToInt32(data);

                        switch (opcion)
                        {
                            case 1:
                                DescargaFichero("FotoMonte", data, sender);
                                break;

                            case 2:
                                DescargaFichero("FotoPlaya", data, sender);
                                break;

                            case 3:
                                DescargaFichero("FotoCiudad", data, sender);
                                break;
                            case 4:
                                Console.WriteLine("Has elegido salir de la aplicación");
                                salir = true;
                                break;
                            default:
                                Console.WriteLine("Elige una opcion entre 1 y 4");
                                break;
                        }

                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                Console.ReadLine();
                sender.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                //Cerramos el socket
                sender.Close();

            }
        }


        //El siguiente método tiene como objetivo realizar el descarga del fichero desde el servidor.
        //@nombreFoto: Se recoge el nombre de la foto.
        //@sender: Se recoge el objeto socket creado para la comunicación con el servidor.
        //@opcion: Recoge la opción del menú en una variable entera.
        private void DescargaFichero(string nombreFoto, string opcion, Socket sender)
        {
            byte[] bytes = new Byte[1024];
            byte[] tamanob = new Byte[16];
            string ruta =string.Empty;
            string data = string.Empty;
            


            //Envío de opción
            Console.WriteLine("Has elegido descargar {0}.", nombreFoto);
         
            Console.WriteLine("Comienza la descarga...espere unos segundos");
            bytes = Encoding.ASCII.GetBytes(opcion);
            sender.Send(bytes);

            //Recoger tamaño fichero
            int bytesRec = sender.Receive(tamanob, sizeof(long), SocketFlags.None);
            data = Encoding.ASCII.GetString(tamanob, 0, bytesRec);
            var tamano = Convert.ToInt32(data);


            //Especificar ruta donde se va a guardar la imagen
            ruta = String.Empty;
            ruta = @"../../../argazkiak/";
            ruta = ruta + nombreFoto + ".jpg";

            try
            {
                //Escritura de fichero en tamaño de bloques
                int bytesReadTotal = 0;
                using (FileStream fs = File.OpenWrite(ruta))
                {

                    while (bytesReadTotal < tamano)
                    {
                        //Recibe fichero en bloques de 1024.
                        int bytesRead = sender.Receive(bytes, bytes.Length, SocketFlags.None);
                        //Guarda bloque de fichero
                        fs.Write(bytes, 0, bytesRead);
                        //Aumenta contados hasta que el total recibido sea igual que el tamaño total del fichero.
                        bytesReadTotal += bytesRead;

                    }

                };
            }catch (FileNotFoundException e)
            {
                Console.WriteLine("Fichero no encontrado: {0}", e.Message);
            }


    //Muestra por consola la descarga del fichero
    Console.WriteLine("Finalizada la descarga, fichero {0} guardado en {1}", nombreFoto, Path.GetFullPath(ruta));
            nombreFoto = string.Empty;

        }
    }
}

