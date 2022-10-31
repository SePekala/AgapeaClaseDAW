using AgapeaDAW.Models;
using AgapeaDAW.Models.Interfaces;

var builder = WebApplication.CreateBuilder(args);

//=======================================
// Add services to the container.
//aqui definimos servicios reutilizables por los clientes cada vez q hagan una peticion al server
//=======================================

//1º inyectamos servicio de acceso a datos usando sqlserver cuando los controladores lo soliciten....
//hay 3 metodos posibles:
// .AddSingleton<interface,clase>() <----- el servicio se crea una unica vez para  TODOS los clientes q usan el portal web
// .AddScoped<interface,clase() <--------- el servicio se crea una unica vez por CADA CLIENTE y se reutiliza en sucesivas peticiones de ese cliente
// .AddTransient<inteface,clase>() <------ el sevicio se crea por cada peticion q haga cada cliente, no se reuitiliza 

builder.Services.AddScoped<IBDAccess, SqlServerDBAccess>();
builder.Services.AddScoped<IClienteEmail, EmailServerMailJetSINAPI>();

//configuracion de cookie de sesion
builder.Services.AddSession(
        (SessionOptions opciones) =>
        {
            opciones.Cookie.HttpOnly = true;
            opciones.Cookie.MaxAge = new TimeSpan(1, 0, 0);
        }
    );

builder.Services.AddControllersWithViews();

var app = builder.Build();



//======================================
// Configure the HTTP request pipeline.
//======================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection(); //<-----1º middleware de la pipeline
app.UseStaticFiles(); //<----------2º middleware de la pipeline

app.UseRouting(); //<---------------3º middleware de la pipeline FUNDAMENTAL, define el enrutamiento

app.UseSession(); //<--------------- 4º middleware de la pipeline USO ESTADO DE SESION por cada cliente q se conecta

app.UseAuthorization(); //<---------4º middleware de la pipeline

//====================================================================
//======= definicion de lista de rutas del middleware de enrutamiento
// ======= el orden es importante pq se chequean por orden en q se añaden a la lista
//======= se añaden usando metodo .MapControllerRoute( parametros_definicion_objeto_Route )
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tienda}/{action=RecuperaLibros}/{id?}"); //<---- detecta solicitudes del cliente q tengan minimo 3 segmentos
//            ----------------  -------------- ------
//              segmento 1        segmento 2     segmento 3 opcional(?) y si aparece lo recibe como parametro el metod
//             id el controlador   metodo dentro    de accion del controlador
//                                del controlador

// cliente pone en navegador:  https://localhost:xxxx/ <------------------ https://localhost:xxxx/Home/Index/
//                             https://localhost:xxxx/Cliente/Registro <---segmento1=Cliente(Controller), segmento2=Registro
//                             https://localhost:xxxx/Tienda/DetallesLibro/1234325-X  <---segmento1=Tienda(Controller), segmento2=DetallesLibro, segmento3=id=123425-X
app.Run();
