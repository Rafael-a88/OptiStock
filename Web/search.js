
function searchProduct(event) {
    event.preventDefault(); // Evita que el formulario se envíe de la manera tradicional
    const query = document.getElementById('searchInput').value.toLowerCase(); // Obtiene el valor del campo de búsqueda

    // Mapa de términos de búsqueda a páginas
    const productPages = {
        // Alimentacion
        'yogur': 'Alimentacion.html',
        'leche': 'Alimentacion.html',
        'coca': 'Alimentacion.html',  
        'cola': 'Alimentacion.html',
        'tonica': 'Alimentacion.html',
        'vino': 'Alimentacion.html',
        'agua': 'Alimentacion.html',

        // Bazar

        'cable': 'Bazar.html',
        'pila': 'Bazar.html',
        'altavoz': 'Bazar.html',
        'auricular': 'Bazar.html',
        'cargador': 'Bazar.html',

        // Decoración

        'estanteria': 'Decoracion.html',
        'vela': 'Decoracion.html',
        'jarron': 'Decoracion.html',
        'plato': 'Decoracion.html',
        'portavela': 'Decoracion.html',
        'zapatilla': 'Decoracion.html',

        // Disfraces

        'melchor': 'Disfraces.html',
        'gaspar': 'Disfraces.html',
        'baltasar': 'Disfraces.html',
        'rey': 'Disfraces.html',
        'paje': 'Disfraces.html',

        // Hogar y Baño

        'espejo': 'HogaryBaño.html',
        'organizador': 'HogaryBaño.html',
        'papelera': 'HogaryBaño.html',
        'cesto': 'HogaryBaño.html',
        'portarollos': 'HogaryBaño.html',
        'dispensador': 'HogaryBaño.html',

         // Hostelería

        'molde': 'Hosteleria.html',
        'tabla': 'Hosteleria.html',
        'bandeja': 'Hosteleria.html',
        'bambu': 'Hosteleria.html',
        'portarollos': 'Hosteleria.html',
        'cacerola': 'Hosteleria.html',

        // Juguetes

        'cerdito': 'Juguetes.html',
        'gallo': 'Juguetes.html',
        'peluche': 'Juguetes.html',
        'policia': 'Juguetes.html',
        'helicoptero': 'Juguetes.html',
        'bomberos': 'Juguetes.html',

        // Navidad

        'peluche': 'Navidad.html',
        'navidad': 'Navidad.html',
        'decoracion': 'Navidad.html',
        'corona': 'Navidad.html',
        'castillo': 'Navidad.html',
        'muñeco': 'Navidad.html',

        // Papelería

        'agenda': 'Papeleria.html',
        'boligrafo': 'Papeleria.html',
        'cuaderno': 'Papeleria.html',
        'mario': 'Papeleria.html',
        'mochila': 'Papeleria.html',
        'portatodo': 'Papeleria.html',
    };

    // Redirige a la página correspondiente si existe
    if (productPages[query]) {
        window.location.href = productPages[query]; // Redirige a la página del producto
    } else {
        alert('Producto no encontrado'); // Mensaje si no se encuentra el producto
    }
}

