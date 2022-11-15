using AgapeaDAW.Models.Interfaces;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

//namespace para encriptar y hashear password cuenta cliente, tambien para comprobar
using BCrypt.Net;

namespace AgapeaDAW.Models
{
    public class SqlServerDBAccess : IBDAccess
    {
        #region ...propiedades de la clase de acceso a datos contra SQL-Server...

        public string CadenaConexion { get; set; }
        private IConfiguration _accesoappsettings; //<--- prop.de la clase para almacenar servicio inyectado de acceso a fichero appsettings.json

        public SqlServerDBAccess(IConfiguration accesoappsettings)
        {
            _accesoappsettings = accesoappsettings;
            this.CadenaConexion=_accesoappsettings.GetSection("ConnectionStrings:SqlServerConnectionString").Value;
        }
        #endregion

        #region ...metodos de la clase de acceso a datos contra SQL-Server...
        #region 1-metodos ClienteController
        public Cliente ComprobarCredenciales(string email, string password)
        {
            //abrir conexion contra BD, lanzar select contra tabla cuentas, comprobar si hash esta ok, 
            try
            {
                SqlConnection _conexionBd = new SqlConnection(this.CadenaConexion);
                _conexionBd.Open();

                SqlCommand _selectCuenta = new SqlCommand("SELECT * FROM dbo.Cuentas WHERE Email=@em", _conexionBd);
                _selectCuenta.Parameters.AddWithValue("@em", email);

                SqlDataReader _cursor = _selectCuenta.ExecuteReader();
                String _idCliente = "";

                Cliente _clienteADevolver = new Cliente();

                if (_cursor.HasRows)
                {
                    while (_cursor.Read())
                    {
                        String _hashpassword = _cursor["Password"].ToString() ?? "";
                        _idCliente = _cursor["IdCliente"].ToString() ?? "";

                        if (!BCrypt.Net.BCrypt.Verify(password, _hashpassword))
                        {
                            throw new Exception("password incorrecta");
                        }
                        else
                        {
                            _clienteADevolver.Credenciales.Login = _cursor["Login"].ToString() ?? "";
                            _clienteADevolver.Credenciales.Email = email;
                            _clienteADevolver.Credenciales.IdCuenta = _cursor["IdCredenciales"].ToString() ?? "";
                            _clienteADevolver.Credenciales.CuentaActiva = System.Convert.ToBoolean(_cursor["CuentaActiva"]);
                            _clienteADevolver.Credenciales.ImagenCuenta = _cursor["ImagenCuenta"].ToString() ?? "";
                            _clienteADevolver.Credenciales.ImagenCuentaBASE64 = _cursor["ImagenCuentaBASE64"].ToString() ?? "";
                            _clienteADevolver.Credenciales.Password = "";

                            break;
                        }
                    }
                    _cursor.Close();

                    //select sobre tabla clientes, select sobre tabla direcciones, pedidos, opiniones, listas
                    SqlCommand _selectCliente = new SqlCommand("SELECT * FROM dbo.Clientes WHERE IdCliente=@id", _conexionBd);
                    _selectCliente.Parameters.AddWithValue("@id", _idCliente);

                    SqlDataReader _cursor2 = _selectCliente.ExecuteReader();

                    while (_cursor2.Read())
                    {
                        _clienteADevolver.IdCliente = _cursor2["IdCliente"].ToString() ?? "";
                        _clienteADevolver.Nombre = _cursor2["Nombre"].ToString() ?? "";
                        _clienteADevolver.Apellidos = _cursor2["Apellidos"].ToString() ?? "";
                        _clienteADevolver.NIF = _cursor2["NIF"].ToString() ?? "";
                        _clienteADevolver.Telefono = _cursor2["Telefono"].ToString() ?? "";
                        _clienteADevolver.FechaNacimiento = System.Convert.ToDateTime(_cursor2["FechaNacimiento"]);
                        _clienteADevolver.Genero = _cursor2["Genero"].ToString() ?? "";
                        _clienteADevolver.Descripcion = _cursor2["Descripcion"].ToString() ?? "";

                    }
                    _cursor2.Close();

                    SqlCommand _selectDirecciones = new SqlCommand("SELECT * FROM dbo.Direcciones WHERE IdCliente=@id", _conexionBd);
                    _selectDirecciones.Parameters.AddWithValue("@id", _clienteADevolver.IdCliente);
                    SqlDataReader _cursorDirec = _selectDirecciones.ExecuteReader();
                    while (_cursorDirec.Read())
                    {
                        Direccion _nuevaDirec = new Direccion
                        {
                            IdDireccion = _cursorDirec["IdDireccion"].ToString(),
                            Calle = _cursorDirec["Calle"].ToString(),
                            CP = System.Convert.ToInt32(_cursorDirec["CP"]),
                            Pais = _cursorDirec["Pais"].ToString(),
                            ProvinciaDirecc = new Provincia
                            {
                                CCOM = "",
                                CPRO = _cursorDirec["Provincia"].ToString().Split('-')[0],
                                PRO = _cursorDirec["Provincia"].ToString().Split('-')[1]
                            },
                            LocalidadDirecc = new Muncipio
                            {
                                CUN = "",
                                CPRO = _cursorDirec["Provincia"].ToString().Split('-')[0],
                                CMUM = _cursorDirec["Municipio"].ToString().Split('-')[0],
                                DMUN50 = _cursorDirec["Municipio"].ToString().Split('-')[1],
                            },
                            EsFacturacion = System.Convert.ToBoolean(_cursorDirec["EsFacturacion"]),
                            EsPrincipal = System.Convert.ToBoolean(_cursorDirec["EsPrincipal"]),
                        };
                        _clienteADevolver.MisDirecciones.Add(_nuevaDirec);
                    }
                    _cursorDirec.Close();
                    _conexionBd.Close();


                    return _clienteADevolver;

                }
                else
                {
                    return null;
                }


            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public bool RegistrarCliente(Cliente newcliente)
        {
            try
            {
                //1º abrir conexion contra la BD
                SqlConnection _conexionBD = new SqlConnection(this.CadenaConexion);
                _conexionBD.Open();

                //2º hacer INSERT en tabla clientes cols:IdCliente,Nombre,Apellidos,NIF,Telefono
                SqlCommand _insertCliente = new SqlCommand();
                _insertCliente.Connection = _conexionBD; //objeto SqlConnetion a usar para ejecutar la consulta

                _insertCliente.CommandText = "INSERT INTO dbo.Clientes VALUES (@id,@nom,@ape,@nif,@tlfno,@fech,@gen,@desc)"; //sentencia a lanzar, el insert
                _insertCliente.Parameters.AddWithValue("@id", newcliente.IdCliente);
                _insertCliente.Parameters.AddWithValue("@nom", newcliente.Nombre);
                _insertCliente.Parameters.AddWithValue("@ape", newcliente.Apellidos);
                _insertCliente.Parameters.AddWithValue("@nif", newcliente.NIF);
                _insertCliente.Parameters.AddWithValue("@tlfno", newcliente.Telefono);
                _insertCliente.Parameters.AddWithValue("@fech", newcliente.FechaNacimiento);
                _insertCliente.Parameters.AddWithValue("@gen", newcliente.Genero);
                _insertCliente.Parameters.AddWithValue("@desc", newcliente.Descripcion);

                int _numfilasInsert = _insertCliente.ExecuteNonQuery();
                if (_numfilasInsert == 1)
                {
                    //3º hacer INSERT en tabla cuentas IdCredencial, IdCliente, Login,Email,Password,CuentaActiva,ImagenCuenta
                    SqlCommand _insertCuenta = new SqlCommand("INSERT INTO dbo.Cuentas VALUES (@idcred,@idcli,@log,@em,@pass,@act,@img)", _conexionBD);
                    _insertCuenta.Parameters.AddWithValue("@idcred", newcliente.Credenciales.IdCuenta);
                    _insertCuenta.Parameters.AddWithValue("@idcli", newcliente.IdCliente);
                    _insertCuenta.Parameters.AddWithValue("@log", newcliente.Credenciales.Login);
                    _insertCuenta.Parameters.AddWithValue("@em", newcliente.Credenciales.Email);

                    String _hashPassword = BCrypt.Net.BCrypt.HashPassword(newcliente.Credenciales.Password);
                    _insertCuenta.Parameters.AddWithValue("@pass", _hashPassword);

                    _insertCuenta.Parameters.AddWithValue("@act", newcliente.Credenciales.CuentaActiva);
                    _insertCuenta.Parameters.AddWithValue("@img", newcliente.Credenciales.ImagenCuenta);

                    // return _insertCuenta.ExecuteNonQuery() == 1 ? true : false;


                    int _numfilasInsertCu = _insertCuenta.ExecuteNonQuery();
                    if (_numfilasInsertCu == 1)
                    {
                        _conexionBD.Close();
                        return true;
                    }
                    else
                    {
                        _conexionBD.Close();
                        return false;
                    }


                }
                else
                {
                    _conexionBD.Close();
                    return false;
                }


                //4º cerrar conexion

            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public bool ActivarCuentaCliente(string idCuenta)
        {
            try
            {
                using (SqlConnection __conexionBD = new SqlConnection(this.CadenaConexion))
                {
                    __conexionBD.Open();
                    SqlCommand __updateCuentas = new SqlCommand("UPDATE dbo.Cuentas SET CuentaActivada=true WHERE IdCuenta=@id", __conexionBD);
                    __updateCuentas.Parameters.AddWithValue("@id", idCuenta);

                    int __filasmodif = __updateCuentas.ExecuteNonQuery();
                    return __filasmodif == 1 ? true : false;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public Cuenta RecuperarCuenta(string idCuenta)
        {
            try
            {
                using (SqlConnection __conexionBD = new SqlConnection(this.CadenaConexion))
                {
                    __conexionBD.Open();
                    SqlCommand __selectCuenta = new SqlCommand("SELECT * FROM dbo.Cuentas WHERE IdCuenta=@id", __conexionBD);
                    __selectCuenta.Parameters.AddWithValue("@id", idCuenta);

                    SqlDataReader __cursor = __selectCuenta.ExecuteReader();

                    Cuenta __cuentaCliente = new Cuenta();
                    while (__cursor.Read())
                    {
                        __cuentaCliente.Login = __cursor["Login"].ToString() ?? "";
                        __cuentaCliente.Password = "";
                        __cuentaCliente.IdCuenta = __cursor["IdCuenta"].ToString() ?? "";
                        __cuentaCliente.ImagenCuenta = __cursor["ImagenCuenta"].ToString() ?? "";
                        __cuentaCliente.Email = __cursor["Email"].ToString() ?? "";
                    }
                    return __cuentaCliente;
                }
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public bool OperarDireccion(Direccion direc, string operacion, string idcliente)
        {
            try
            {
                SqlConnection _conexionBD = new SqlConnection(this.CadenaConexion);
                _conexionBD.Open();

                SqlCommand _comandoDirec = new SqlCommand();
                _comandoDirec.Connection = _conexionBD;
                switch (operacion)
                {
                    case "crear":
                        _comandoDirec.CommandText = "INSERT INTO dbo.Direcciones VALUES(@id,@idc,@c,@cp,@p,@m,@pa,0,0)";
                        _comandoDirec.Parameters.AddWithValue("@idc", idcliente);
                        _comandoDirec.Parameters.AddWithValue("@c", direc.Calle);
                        _comandoDirec.Parameters.AddWithValue("@cp", direc.CP);
                        _comandoDirec.Parameters.AddWithValue("@p", direc.ProvinciaDirecc.CPRO + "-" + direc.ProvinciaDirecc.PRO);
                        _comandoDirec.Parameters.AddWithValue("@m", direc.LocalidadDirecc.CMUM + "-" + direc.LocalidadDirecc.DMUN50);
                        _comandoDirec.Parameters.AddWithValue("@pa", direc.Pais);

                        break;

                    case "modificar":
                        _comandoDirec.CommandText = "UPDATE dbo.Direcciones SET Calle=@c,CP=@cp,Provincia=@p,Municipio=@m,Pais=@pa WHERE IdDireccion=@id";
                        _comandoDirec.Parameters.AddWithValue("@c", direc.Calle);
                        _comandoDirec.Parameters.AddWithValue("@cp", direc.CP);
                        _comandoDirec.Parameters.AddWithValue("@p", direc.ProvinciaDirecc.CPRO + "-" + direc.ProvinciaDirecc.PRO);
                        _comandoDirec.Parameters.AddWithValue("@m", direc.LocalidadDirecc.CMUM + "-" + direc.LocalidadDirecc.DMUN50);
                        _comandoDirec.Parameters.AddWithValue("@pa", direc.Pais);

                        break;

                    case "borrar":
                        _comandoDirec.CommandText = "DELETE FROM dbo.Direcciones WHERE IdDireccion=@id";
                        break;
                }
                _comandoDirec.Parameters.AddWithValue("@id", direc.IdDireccion);


                int _numfilas = _comandoDirec.ExecuteNonQuery();
                return _numfilas == 1;


            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public bool AlmacenaFichImagen(string idCuenta, string idCliente, string nombreFichero, String base64Img)
        {
            try
            {
                SqlConnection _conexionBD = new SqlConnection(this.CadenaConexion);
                _conexionBD.Open();

                SqlCommand _updateCuentas = new SqlCommand("UPDATE dbo.Cuentas SET ImagenCuenta=@img, ImagenCuentaBASE64=@img64 WHERE IdCredenciales=@id AND IdCliente=@idc", _conexionBD);
                _updateCuentas.Parameters.AddWithValue("@img", nombreFichero);
                _updateCuentas.Parameters.AddWithValue("@img64", base64Img);
                _updateCuentas.Parameters.AddWithValue("@id", idCuenta);
                _updateCuentas.Parameters.AddWithValue("@idc", idCliente);

                int _numfilas = _updateCuentas.ExecuteNonQuery();
                return _numfilas == 1;

            }
            catch (Exception ex)
            {

                return false;
            }
        }

        #endregion

        #region 2-metodos TiendaController
        public List<Libro> RecuperaLibros(string idcategoria)
        {
            try
            {
                SqlConnection conexionBD = new SqlConnection(this.CadenaConexion);
                conexionBD.Open();

                SqlCommand selectLibros = new SqlCommand("SELECT * FROM dbo.Libros WHERE IdCategoria LIKE @idc + '%' " ,conexionBD);
                selectLibros.Parameters.AddWithValue("@idc", idcategoria);

                return selectLibros.ExecuteReader().Cast<IDataRecord>()
                                            .Select((IDataRecord fila) => new Libro { 
                                                Titulo=fila["Titulo"].ToString(),
                                                Edicion= fila["Edicion"].ToString(),
                                                Editorial = fila["Editorial"].ToString(),
                                                Autores = fila["Autores"].ToString(),
                                                Dimensiones = fila["Dimensiones"].ToString(),
                                                IdCategoria=idcategoria,
                                                Idioma = fila["Idioma"].ToString(),
                                                ImagenLibro = fila["ImagenLibro"].ToString(),
                                                ImagenLibroBASE64 = fila["ImagenLibroBASE64"].ToString(),
                                                ISBN10 = fila["ISBN10"].ToString(),
                                                ISBN13 = fila["ISBN13"].ToString(),
                                                NumeroPaginas = System.Convert.ToInt32(fila["NumeroPaginas"]),
                                                Resumen = fila["Resumen"].ToString(),
                                                Precio = System.Convert.ToDecimal(fila["Precio"])
                                            })
                                            .ToList<Libro>();
            }
            catch (Exception ex)
            {

                return new List<Libro>();
            }
        }

        public List<Categoria> RecuperaCategorias(String idCategoria)
        {
            //en idCategoria puede ir o bien "Raiz" desde el _layout y entonces tengo q hacer la select por idcategoria buscando aquellos q no tienen -
            //o bien puede ir el id de una categoria, p.e "Informatica" y tendria q recuperar las subcategorias q la pertenecen
            try
            {
                SqlConnection conexionBD = new SqlConnection(this.CadenaConexion);
                conexionBD.Open();
                
                SqlCommand _selectCat = new SqlCommand("SELECT * FROM dbo.Categorias", conexionBD);
                
                return _selectCat.ExecuteReader().Cast<IDataRecord>()
                                                 .Select((IDataRecord fila)=> new Categoria { 
                                                     IdCategoria=fila["IdCategoria"].ToString(),
                                                     NombreCategoria=fila["NombreCategoria"].ToString(),
                                                 })
                                                 .Where((Categoria cat) => {
                                                     //el where va a seleccionar de todos los objeto categoria creados en el select a partir de cada fila del cursor
                                                     //aquellas q cumplan la condicion q cumpla en esta funcion
                                                     if (idCategoria == "Raiz") {
                                                         return new Regex("^[0-9]{1,}$").IsMatch(cat.IdCategoria);
                                                     } else {
                                                         return new Regex("^" + idCategoria + "-.*").IsMatch(cat.IdCategoria);
                                                     }
                                                 })
                                                 .ToList<Categoria>();

            }
            catch (Exception ex)
            {

                return new List<Categoria>();
            }
        }
        #endregion

        #region 3-metodos PedidoController
        public Libro RecuperaLibroISBN(string isbn13)
        {
            try
            {
                SqlConnection _conexionBD = new SqlConnection(this.CadenaConexion);
                _conexionBD.Open();

                SqlCommand _selectLibro = new SqlCommand("SELECT * FROM dbo.Libros WHERE ISBN13=@id", _conexionBD);
                _selectLibro.Parameters.AddWithValue("@id", isbn13);
                return _selectLibro.ExecuteReader().Cast<IDataRecord>().Select((IDataRecord fila) => new Libro
                {
                    Titulo = fila["Titulo"].ToString(),
                    Edicion = fila["Edicion"].ToString(),
                    Editorial = fila["Editorial"].ToString(),
                    Autores = fila["Autores"].ToString(),
                    Dimensiones = fila["Dimensiones"].ToString(),
                    IdCategoria = fila["IdCategoria"].ToString(),
                    Idioma = fila["Idioma"].ToString(),
                    ImagenLibro = fila["ImagenLibro"].ToString(),
                    ImagenLibroBASE64 = fila["ImagenLibroBASE64"].ToString(),
                    ISBN10 = fila["ISBN10"].ToString(),
                    ISBN13 = fila["ISBN13"].ToString(),
                    NumeroPaginas = System.Convert.ToInt32(fila["NumeroPaginas"].ToString()),
                    Resumen = fila["Resumen"].ToString(),
                    Precio = System.Convert.ToDecimal(fila["Precio"])
                }).Single<Libro>();
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion


        #endregion

    }
}
