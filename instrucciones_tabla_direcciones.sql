--create table dbo.Direcciones (
--	[IdDireccion] varchar(100) not null primary key,
--	[IdCliente] varchar(100) not null,
--	[Calle] varchar(max),
--	[CP] int,
--	[Provincia] varchar(max),
--	[Municipio] varchar(max),
--	[Pais] varchar(max),
--	[EsPrincipal] bit,
--	[EsFacturacion] bit

--)

--select IdCliente from dbo.Clientes;

--insert into dbo.Direcciones values(
--'001699c2-98e2-4f4e-8d9f-e3e6df8f5001',
--'624699c2-98e2-4f4e-8d9f-e3e6df8f5a92',
--'Avenida Complutense numero 1, piso 4ºA',
--28802,
--'28-COMUNIDAD DE MADRID',
--'003-ALCALA DE HENARES',
--'España',
--0,
--0
--)
select * from dbo.Direcciones;