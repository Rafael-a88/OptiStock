document.addEventListener('DOMContentLoaded', () => {
  let cart = JSON.parse(localStorage.getItem('cart')) || [];
  const cartItems = document.getElementById('cart-items');
  const totalPriceElement = document.getElementById('total-price');
  const finalizarCompraButton = document.getElementById('finalizarCompraButton');

  function displayCart() {
      cartItems.innerHTML = '';
      let totalPrice = 0;

      cart.forEach(item => {
          let totalItemPrice = item.price * item.quantity;
          totalPrice += totalItemPrice;

          let row = document.createElement('tr');
          row.innerHTML = `
              <td>${item.name}</td>
              <td>${item.price}€</td>
              <td>
                  <button class="quantity-btn" data-product-name="${item.name}" data-change="-1"> - </button>
                  &nbsp;${item.quantity}&nbsp;
                  <button class="quantity-btn" data-product-name="${item.name}" data-change="1"> + </button>
              </td>
              <td>${totalItemPrice.toFixed(2)}€</td>
              <td><button class="btn btn-danger btn-sm remove-item-btn" data-product-name="${item.name}">Eliminar</button></td>
          `;
          cartItems.appendChild(row);
      });

      totalPriceElement.textContent = totalPrice.toFixed(2) + '€';
      addEventListenersToCartButtons();
  }

  function updateQuantity(productName, change) {
      let productIndex = cart.findIndex(item => item.name === productName);

      if (productIndex !== -1) {
          cart[productIndex].quantity += change;
          if (cart[productIndex].quantity <= 0) {
              cart.splice(productIndex, 1);
          }
          localStorage.setItem('cart', JSON.stringify(cart));
          displayCart();
      }
  }

  function removeItem(productName) {
      cart = cart.filter(item => item.name !== productName);
      localStorage.setItem('cart', JSON.stringify(cart));
      displayCart();
  }

  function addEventListenersToCartButtons() {
      const quantityButtons = document.querySelectorAll('.quantity-btn');
      quantityButtons.forEach(button => {
          button.removeEventListener('click', handleQuantityChange);
          button.addEventListener('click', handleQuantityChange);
      });

      const removeButtons = document.querySelectorAll('.remove-item-btn');
      removeButtons.forEach(button => {
          button.removeEventListener('click', handleRemoveItem);
          button.addEventListener('click', handleRemoveItem);
      });
  }

  function handleQuantityChange(event) {
      const productName = event.target.dataset.productName;
      const change = parseInt(event.target.dataset.change);
      updateQuantity(productName, change);
  }

  function handleRemoveItem(event) {
      const productName = event.target.dataset.productName;
      removeItem(productName);
  }

  finalizarCompraButton.addEventListener('click', finalizarCompra);

  async function finalizarCompra() {
      if (cart.length === 0) {
          alert("El carrito está vacío. No hay productos para finalizar la compra.");
          return;
      }

      // Obtener datos del cliente desde localStorage
      const clienteWeb = {
          nombre: localStorage.getItem('usuarioNombre'),
          apellido: localStorage.getItem('usuarioApellido'),
          direccion: localStorage.getItem('usuarioDireccion'),
          ciudad: localStorage.getItem('usuarioCiudad'),
          correo: localStorage.getItem('usuarioCorreo'),
      };

      if (!validarDatosCliente(clienteWeb)) {
          alert("Por favor, completa todos los campos correctamente.");
          return; 
      }

      const pedido = {
          numeroPedido: generarNumeroPedido(),
          clienteWeb,
          precioTotal: parseFloat(totalPriceElement.textContent),
          estado: "Pendiente"
      };

      const detallesPedido = cart.map(item => ({
          nombreProducto: item.name,
          cantidad: item.quantity,
          precioTotal: item.price * item.quantity
      }));

      try {
          await enviarPedido(pedido, detallesPedido);
          alert("¡Compra finalizada con éxito!");
          localStorage.removeItem('cart');
          displayCart();
      } catch (error) {
          alert("Error al finalizar la compra: " + error);
          console.error("Error al finalizar la compra:", error);
      }
  }

  function generarNumeroPedido() {
      return "PED-" + Math.floor(Math.random() * 1000000);
  }

  async function enviarPedido(pedido, detallesPedido) {
      try {
          const response = await fetch('http://localhost:3000/pedidos', {
              method: 'POST',
              headers: {
                  'Content-Type': 'application/json'
              },
              body: JSON.stringify({ pedido, detallesPedido })
          });

          if (!response.ok) {
              throw new Error('Error al enviar el pedido al servidor. Código de estado: ' + response.status);
          }

          return await response.json();
      } catch (error) {
          throw new Error('Error al procesar la solicitud de pedido: ' + error.message);
      }
  }

  function validarDatosCliente(cliente) {
      // Validar que todos los campos estén completos
      return Object.values(cliente).every(value => value && value.trim() !== '');
  }

  displayCart(); // Inicializa la visualización del carrito al cargar la página
});
