using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PSP03_Socket_TCP
{
    internal class Servidor
    {
        public static int Main(String[] args)
        {

            Servidor servidor = new Servidor();
            servidor.FuncionCliente();

            Console.WriteLine("Pulse intro para continuar");
            Console.ReadLine();

            return 0;
        }
        private void FuncionCliente()
        {
            Socket listener = null;
            Socket handler = null;
            

            try
            {
                //declaramos el puerto
                int port = 12000;
                string data = null;

                //buffer
                byte[] opcion = new Byte[1028];

                //Recogemos la IP del servidor
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[1];
                
                //Creación del socket listener para recepcionar las peticiones del cliente
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("Programa servidor iniciando.");

                //Asociamos el socket al puerto e ip del servidor
                IPEndPoint iPEndPoint = new IPEndPoint(ipAddress.Address, port);
                listener.Bind(iPEndPoint);

                //Quedamos a la escucha de un máximo de peticiones de cliente de 10 (en este caso sólo se trabajará con 1 cliente).
                listener.Listen(10);

                //Se establece la conexión con el cliente y abre un segundo socket para la comunicación
                handler = listener.Accept(); //Bloqueante.
                Console.WriteLine("Aceptada la conexión con el  cliente.");

                bool salir = false;

                //Recepción de información
                while (!salir)
                {
                    int bytesRec = handler.Receive(opcion); //El cliente envía la opción a elegir
                    data += Encoding.ASCII.GetString(opcion, 0, bytesRec);

                    //En base al menú transmitimos una imagen u otra.
                    switch (data)
                    {
                        case "1":
                            EnviarFichero("FotoMonte", handler);
                            data = String.Empty; //Reinicialmos el dato de selección de opción
                            break;

                        case "2":
                            EnviarFichero("FotoPlaya", handler);
                            data = String.Empty; 
                            break;         

                        case "3":
                            EnviarFichero("FotoCiudad", handler);
                            data = String.Empty; 
                            break;

                        case "4":
                            data = String.Empty;
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("Se cierra por error.");
                            salir = true;
                            data = String.Empty;
                            break;
                    }

                }
                Console.ReadLine();
                handler.Close();
                listener.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                handler.Close();
                listener.Close();
            }

        }

        //El siguiente método tiene como objetivo realizar el envío del fichero a cliente.
        //@nombreFoto: Se recoge el nombre de la foto.
        //@handler: Se recoge el objeto socket creado para la comunicación con el cliente.
        private void EnviarFichero(string nombreFoto, Socket handler)
        {
            FileInfo fileinfo = null;
            long tamano = long.MinValue;
            
            string ruta = String.Empty;
            //Creamos el buffer para el envío y recepción de información
            byte[] bytes = new Byte[1028];
            

            ruta = @"../../../argazkiakhartu/" + nombreFoto + ".jpg";
            Console.WriteLine("Foto enviada con ruta:{0}", ruta);

            //Recogemos tamaño del fichero (El fichero que se genere en destino sea igual que el origen)
            fileinfo = new FileInfo(ruta);
            tamano = fileinfo.Length;

            bytes = Encoding.ASCII.GetBytes(Convert.ToString(tamano));

            //Se envía tamaño de fichero
            handler.Send(bytes, bytes.Length, SocketFlags.None);

            //Comienza el envío de la imagen
            try
            {
                int bytesReadTotal = 0; //init de variable para el control del tamaño del fichero

                //Lectura de fichero en bloques de 1024bytes y transmisión de cada bloque
                using (FileStream fs = File.OpenRead(ruta))
                {
                    while (bytesReadTotal < tamano)
                    {
                        //Lectura de fichero en bloques de 1024. El bloque se guarda en el buffer denominado bytes
                        int bytesRead = fs.Read(bytes, 0, bytes.Length);

                        //Envío de bloque leido (bytes)
                        handler.Send(bytes, bytesRead, SocketFlags.None);

                        //Aumenta el contador del tamaño del fichero leido, hasta que alcance el total
                        bytesReadTotal += bytesRead;
                    }

                };
            }catch (FileNotFoundException e)
            {
                Console.WriteLine("Fichero no encontrado: {0}", e.Message);
            }

        }
    }
}

