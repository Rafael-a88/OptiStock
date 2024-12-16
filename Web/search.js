
function searchProduct(event) {
    event.preventDefault(); // Evita que el formulario se envíe de la manera tradicional
    const query = document.getElementById('searchInput').value.toLowerCase(); // Obtiene el valor del campo de búsqueda

    // Mapa de términos de búsqueda a páginas
    const productPages = {
        'yogur': 'Alimentacion.html',
        'leche': 'Alimentacion.html',
        'pan': 'Alimentacion.html',
        'frutas': 'Alimentacion.html',
        'verduras': 'Alimentacion.html',
        'carne': 'Alimentacion.html',
        // Agrega más productos según sea necesario
    };

    // Redirige a la página correspondiente si existe
    if (productPages[query]) {
        window.location.href = productPages[query]; // Redirige a la página del producto
    } else {
        alert('Producto no encontrado'); // Mensaje si no se encuentra el producto
    }
}

