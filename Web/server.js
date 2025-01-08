const express = require('express');
const mysql = require('mysql');
const bodyParser = require('body-parser');
const cors = require('cors');

const app = express();
const port = 3000;

// Middleware
app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(express.static('public'));

// Conexión a la base de datos
const conexion = mysql.createConnection({
    host: 'localhost',
    user: 'root', // Cambia esto
    password: 'root', // Cambia esto
    database: 'optistock' // Cambia esto
});

conexion.connect(error => {
    if (error) {
        throw error;
    } else {
        console.log('Conexión Exitosa');
    }
});

// Ruta para insertar un cliente
app.post('/registrar', (req, res) => {
    const { nombre, apellidos, direccion, ciudad, correo, contraseña } = req.body;

    // Validación simple
    if (!nombre || !apellidos || !direccion || !ciudad || !correo || !contraseña) {
        return res.status(400).send('Todos los campos son obligatorios.');
    }

    const sql = 'INSERT INTO clienteweb (Nombre, Apellido, Direccion, Ciudad, Correo, Contraseña) VALUES (?, ?, ?, ?, ?, ?)';
    conexion.query(sql, [nombre, apellidos, direccion, ciudad, correo, contraseña], (err, result) => {
        if (err) {
            console.error('Error en la consulta:', err);
            return res.status(500).send('Error en el registro: ' + err.message);
        }
        res.send('Registro completado exitosamente');
    });
});

// Ruta para iniciar sesión
app.post('/login', (req, res) => {
    const { correo, contraseña } = req.body;

    // Validación simple
    if (!correo || !contraseña) {
        return res.status(400).send('Correo y contraseña son obligatorios.');
    }

    const sql = 'SELECT * FROM clienteweb WHERE Correo = ? AND Contraseña = ?';
    conexion.query(sql, [correo, contraseña], (err, results) => {
        if (err) {
            console.error('Error en la consulta:', err);
            return res.status(500).send('Error en el inicio de sesión: ' + err.message);
        }
        if (results.length > 0) {
            res.send({
                success: true,
                usuario: {
                    ID: results[0].Id,
                    Nombre: results[0].Nombre,
                    Apellido: results[0].Apellido,
                    Direccion: results[0].Direccion,
                    Correo: results[0].Correo,
                    Ciudad: results[0].Ciudad
                }
            });
        } else {
            res.send({ success: false });
        }
    });
});

// Ruta para obtener el ID del cliente por correo
app.get('/cliente/:correo', (req, res) => {
    const correo = req.params.correo;
    const sql = 'SELECT Id FROM clienteweb WHERE Correo = ?';
    conexion.query(sql, [correo], (err, results) => {
        if (err) {
            console.error('Error al obtener el cliente:', err);
            return res.status(500).send('Error al obtener el cliente');
        }
        if (results.length > 0) {
            res.json({ id: results[0].Id });
        } else {
            res.status(404).send('Cliente no encontrado');
        }
    });
});


app.post('/pedidos', async (req, res) => {
    console.log('Solicitud recibida:', req.body);
    const { pedido, detallesPedido } = req.body;

    // Validación de datos
    if (!pedido || !detallesPedido || detallesPedido.length === 0) {
        console.error('Datos del pedido o detalles faltantes.');
        return res.status(400).json({ error: 'Datos del pedido o detalles faltantes.' });
    }

    const { numeroPedido, precioTotal, estado, clienteWeb } = pedido;

    // Validar campos básicos del pedido
    if (!numeroPedido || !precioTotal || !estado || !clienteWeb || !clienteWeb.correo) {
        console.error('Faltan campos obligatorios en el pedido');
        return res.status(400).json({ error: 'Faltan campos obligatorios en el pedido.' });
    }

    try {
        // 1. Obtener el ID del cliente
        const getClienteId = () => {
            return new Promise((resolve, reject) => {
                const sqlBuscarCliente = 'SELECT Id FROM clienteweb WHERE Correo = ?';
                console.log('Buscando cliente con correo:', clienteWeb.correo);

                conexion.query(sqlBuscarCliente, [clienteWeb.correo], (err, resultadoCliente) => {
                    if (err) {
                        console.error('Error en la consulta de cliente:', err);
                        return reject(err);
                    }

                    console.log('Resultado búsqueda cliente:', resultadoCliente);

                    if (!resultadoCliente || resultadoCliente.length === 0) {
                        return reject(new Error(`Cliente no encontrado con correo: ${clienteWeb.correo}`));
                    }

                    resolve(resultadoCliente[0].Id);
                });
            });
        };

        // 2. Obtener los IDs de los productos (actualizado para buscar por nombre o EAN)
        const getProductoId = (detalle) => {
            return new Promise((resolve, reject) => {
                const sqlBuscarProducto = 'SELECT Id, Stock FROM productos WHERE Nombre = ? OR EAN = ?';
                console.log('Buscando producto:', detalle.nombreProducto, 'o EAN:', detalle.ean);

                conexion.query(sqlBuscarProducto, [detalle.nombreProducto, detalle.ean], (err, resultadoProducto) => {
                    if (err) {
                        console.error('Error en la consulta de producto:', err);
                        return reject(err);
                    }

                    console.log('Resultado búsqueda producto:', resultadoProducto);

                    if (!resultadoProducto || resultadoProducto.length === 0) {
                        return reject(new Error(`Producto no encontrado: ${detalle.nombreProducto} o EAN: ${detalle.ean}`));
                    }

                    resolve(resultadoProducto[0]);
                });
            });
        };

        // Obtener el ID del cliente
        console.log('Iniciando búsqueda de cliente...');
        const clienteWebId = await getClienteId();
        console.log('ID del cliente encontrado:', clienteWebId);

        // Obtener los IDs de todos los productos
        console.log('Iniciando búsqueda de productos...');
        const detallesConIds = await Promise.all(
            detallesPedido.map(async (detalle) => {
                try {
                    const producto = await getProductoId(detalle);
                    console.log(`Producto "${detalle.nombreProducto}" encontrado con ID:`, producto.Id);
                    return {
                        ...detalle,
                        productoId: producto.Id,
                        stock: producto.Stock // Guardamos el stock disponible
                    };
                } catch (error) {
                    throw new Error(`Error al buscar producto "${detalle.nombreProducto}": ${error.message}`);
                }
            })
        );

        // Función para encontrar el primer ID libre
        const getNextFreeId = async () => {
            return new Promise((resolve, reject) => {
                const sqlCheckId = 'SELECT Id FROM pedidos_web ORDER BY Id ASC'; // Obtener todos los IDs ordenados
                conexion.query(sqlCheckId, (err, result) => {
                    if (err) {
                        return reject(err);
                    }

                    console.log('IDs encontrados en la tabla pedidos_web:', result);

                    // Si no hay resultados, significa que la tabla está vacía y el primer ID libre es 1
                    if (result.length === 0) {
                        console.log('No hay pedidos existentes. El primer ID libre es 1.');
                        return resolve(1);
                    }

                    // Buscar el primer ID libre en la secuencia
                    for (let i = 1; i <= result.length + 1; i++) {
                        if (!result.some((row) => row.Id === i)) {
                            console.log('Primer ID libre encontrado:', i);
                            return resolve(i); // Retorna el primer ID libre encontrado
                        }
                    }
                });
            });
        };

        // Obtener el próximo ID libre
        const nuevoIdPedido = await getNextFreeId();
        console.log('Nuevo ID de pedido asignado:', nuevoIdPedido);

        // Iniciar la transacción para crear el pedido
        conexion.beginTransaction(async (err) => {
            if (err) {
                console.error('Error al iniciar la transacción:', err);
                return res.status(500).json({ error: 'Error al procesar la solicitud de pedido.' });
            }

            try {
                // Insertar el pedido con el nuevo ID libre
                const resultPedido = await new Promise((resolve, reject) => {
                    const sqlPedido = 'INSERT INTO pedidos_web (Id, NumeroPedido, ClienteWebId, PrecioTotal, Estado) VALUES (?, ?, ?, ?, ?)';
                    console.log('Insertando pedido con datos:', {
                        nuevoIdPedido,
                        numeroPedido,
                        clienteWebId,
                        precioTotal,
                        estado
                    });

                    conexion.query(sqlPedido, [nuevoIdPedido, numeroPedido, clienteWebId, precioTotal, estado], (err, result) => {
                        if (err) {
                            console.error('Error al insertar pedido:', err);
                            return reject(err);
                        }
                        resolve(result);
                    });
                });

                console.log('Pedido insertado con ID:', nuevoIdPedido); // Imprimir el ID del pedido insertado

                // Insertar detalles del pedido y actualizar stock
                for (const detalle of detallesConIds) {
                    console.log('Insertando detalle:', {
                        pedidoId: nuevoIdPedido,
                        productoId: detalle.productoId,
                        cantidad: detalle.cantidad,
                        stock: detalle.stock // Mostrar el stock del producto
                    });

                    // Verificar si hay suficiente stock
                    if (detalle.stock < detalle.cantidad) {
                        console.error(`Stock insuficiente para el producto ID ${detalle.productoId}. Stock actual: ${detalle.stock}, cantidad solicitada: ${detalle.cantidad}`);
                        return conexion.rollback(() => {
                            res.status(400).json({ error: `Stock insuficiente para el producto "${detalle.nombreProducto}".` });
                        });
                    }

                    try {
                        // Insertar el detalle en la tabla detalle_pedido
                        await new Promise((resolve, reject) => {
                            const sqlDetalle = 'INSERT INTO detalle_pedido (PedidoId, ProductoId, Cantidad) VALUES (?, ?, ?)';
                            conexion.query(sqlDetalle, [nuevoIdPedido, detalle.productoId, detalle.cantidad], (err, result) => {
                                if (err) {
                                    console.error('Error al insertar detalle:', err);
                                    return reject(err);
                                }
                                resolve(result);
                            });
                        });

                        // Descontar el stock del producto
                        await new Promise((resolve, reject) => {
                            const sqlActualizarStock = 'UPDATE productos SET Stock = Stock - ? WHERE Id = ?';
                            conexion.query(sqlActualizarStock, [detalle.cantidad, detalle.productoId], (err, result) => {
                                if (err) {
                                    console.error('Error al actualizar el stock:', err);
                                    return reject(err);
                                }
                                resolve(result);
                            });
                        });

                    } catch (error) {
                        console.error('Error al insertar detalle o actualizar stock:', error);
                        // Revertir la transacción si hay un error al insertar un detalle
                        return conexion.rollback(() => {
                            res.status(500).json({ error: 'Error al procesar la solicitud de pedido.' });
                        });
                    }
                }

                // Confirmar la transacción
                conexion.commit((err) => {
                    if (err) {
                        console.error('Error al confirmar la transacción:', err);
                        return conexion.rollback(() => {
                            res.status(500).json({ error: 'Error al procesar la solicitud de pedido.' });
                        });
                    }
                    console.log('Transacción completada con éxito.');
                    res.status(201).json({ message: 'Pedido creado con éxito.' });
                });
            } catch (error) {
                console.error('Error durante la transacción:', error);
                return conexion.rollback(() => {
                    res.status(500).json({ error: 'Error al procesar la solicitud de pedido.' });
                });
            }
        });
    } catch (error) {
        console.error('Error en la creación del pedido:', error);
        return res.status(500).json({ error: 'Error al procesar la solicitud de pedido.' });
    }
});



// Iniciar el servidor
app.listen(port, () => {
    console.log(`Servidor escuchando en http://localhost:${port}`);
});
