document.getElementById('loginForm').addEventListener('submit', async function(event) {
    event.preventDefault(); 

    const correo = document.getElementById('correo').value;
    const contraseña = document.getElementById('contraseña').value;

    try {
        const response = await fetch('http://localhost:3000/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ correo, contraseña })
        });

        const result = await response.json();
        console.log(result); // Para depurar

        if (result.success) {
            alert('¡Inicio de sesión exitoso!');

            // Obtener datos del usuario
            const usuarioID = result.usuario.ID; 
            const usuarioNombre = result.usuario.Nombre;
            const usuarioApellido = result.usuario.Apellido;
            const usuarioDireccion = result.usuario.Direccion;
            const usuarioCorreo = result.usuario.Correo;    
            const usuarioCiudad = result.usuario.Ciudad;

            // Verificar que Ciudad no esté vacío
            if (!usuarioCiudad) {
                alert('Error: Ciudad no está definida en la respuesta del servidor.');
                return;
            }

            // Guardar datos del usuario en localStorage
            localStorage.setItem('usuarioID', usuarioID);
            localStorage.setItem('usuarioNombre', usuarioNombre);
            localStorage.setItem('usuarioApellido', usuarioApellido);
            localStorage.setItem('usuarioDireccion', usuarioDireccion);
            localStorage.setItem('usuarioCorreo', usuarioCorreo);
            localStorage.setItem('usuarioCiudad', usuarioCiudad);

            window.location.href = 'index.html';
           
        } else {
            alert('Correo o contraseña incorrectos'); 
        }
} catch (error) {
        console.error('Error en la solicitud:', error);
        alert('Ocurrió un error al intentar iniciar sesión.');
    }
});
