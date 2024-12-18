// Registro.js
document.getElementById('registroForm').addEventListener('submit', function(event) {
    event.preventDefault(); // Evita el comportamiento por defecto del formulario

    const nombre = document.getElementById('nombre').value;
    const apellidos = document.getElementById('apellido').value; // Cambiado de 'apellidos' a 'apellido'
    const direccion = document.getElementById('direccion').value;
    const ciudad = document.getElementById('ciudad').value;
    const correo = document.getElementById('correoRegistro').value;
    const contraseña = document.getElementById('contraseñaRegistro').value;

     // Validación de nombre y apellido (solo letras)
     const nombreRegex = /^[A-Za-záéíóúÁÉÍÓÚñÑ\s]+$/;
     if (!nombreRegex.test(nombre)) {
         alert('El nombre solo debe contener letras.');
         return;
     }
 
    
    // Validación de apellido (letras y espacios)
    const apellidoRegex = /^[A-Za-záéíóúÁÉÍÓÚñÑ\s]+$/; // Añade \s para permitir espacios
    if (!apellidoRegex.test(apellidos)) {
        alert('El apellido solo debe contener letras y espacios.');
        return;
    }
 
     // Validación de correo (debe contener un @)
     if (!correo.includes('@')) {
         alert('El correo debe contener un símbolo @.');
         return;
     }
 

    // Enviar la solicitud POST al servidor
    fetch('http://localhost:3000/registrar', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ nombre, apellidos, direccion, ciudad, correo, contraseña })
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Error en el registro');
        }
        return response.text();
    })
    .then(data => {
        alert(data); // Mostrar el mensaje de éxito
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Error en el registro: ' + error.message);
    });
});
