# Central de repartidores — Composición sobre herencia y Open/Closed

Somos la central de una empresa de repartos. Los pedidos aparecen en los restaurantes de la
ciudad y nosotros decidimos qué repartidor los lleva. Cada repartidor sale de la central,
recoge el pedido, lo entrega en la casa del cliente y regresa a estacionarse. Mientras
regresa, ya se le puede asignar otro pedido.

Durante la clase, el cliente nos va a pedir cambios. Cada paso es uno de esos pedidos.

## Cómo se juega

- **Play** en la escena `Assets/Scenes/City.unity`.
- Haz clic en un **pedido** (izquierda) y después en un **repartidor** (derecha).
- `Esc` cancela la selección. `N` salta al atardecer o al amanecer.
- El botón **Mejorar · Q80** le compra un mejor vehículo a un repartidor.
- Ganancia de cada entrega = lo que paga el cliente − la tarifa del repartidor. Si el
  pedido llega tarde, el cliente paga la mitad.

## Reglas

- El código de los repartidores está en `Assets/Scripts/Couriers/`. Ahí trabajamos.
- `Assets/Scripts/Game/` (calles, pedidos, interfaz, reloj) ya funciona. Solo se modifica
  si el compilador no nos deja otra opción, y si pasa, hay que anotarlo.
- Después de cada paso, el juego tiene que compilar y funcionar.

## Datos

| Vehículo | Velocidad (unidades/s) | Tarifa por entrega | Modelo | Se mejora a |
|---|---|---|---|---|
| Carro | 20 | Q15 + Q4 por km | `Vehicles/Car` | (ya es el mejor) |
| Bici | 10 | Q8 + Q2 por km | `Vehicles/Bike` | Carro |
| Moto | 16 | Q10 + Q3 por km | `Vehicles/Motorcycle` | Carro |

Una mejora cuesta Q80.

| Turno | Trabaja | Recargo por entrega |
|---|---|---|
| Día | de 6:00 a 19:00 | Q0 |
| Noche | de 19:00 a 6:00 | Q5 |

---

## Paso 0 · Lo que ya tenemos

Hay una sola repartidora, Ana, en carro. Juega unos minutos y lee `CarCourier.cs`.

**Preguntas**
1. ¿Qué partes de `CarCourier` dependen de que sea un carro, y cuáles haría cualquier
   repartidor, sin importar su vehículo?
2. ¿Qué otras clases del proyecto mencionan `CarCourier` por nombre?
3. Con un solo carro, ¿alcanza a entregar todos los pedidos? ¿Qué le pedirías al cliente?

## Paso 1 · "Queremos bicis"

> Contraten a **Sofía**, que reparte en bici. Es más lenta que un carro, pero más barata.

**Requisitos**
- Sofía aparece en la lista de repartidores y se le pueden asignar pedidos.
- Se ve en el mapa con el modelo de bici y anda a la velocidad de una bici.
- Cobra la tarifa de bici.
- Lo que hace cualquier repartidor (salir, recoger, entregar, regresar) no se copia y pega.

**Preguntas**
1. ¿Qué archivos tuviste que modificar, aunque ya funcionaban? ¿Por qué no bastó con
   agregar código nuevo?
2. Elige un pedido: ¿cuánto cobra Ana y cuánto Sofía? ¿Cuándo conviene cada una?
3. ¿Qué tendrías que modificar si mañana llega otro tipo de vehículo?

## Paso 2 · "Turno de noche"

> Los repartidores que ya tenemos trabajan solo de día. Contraten a **Marta** (carro) y a
> **Diego** (bici) para el turno de noche. De noche se cobra un recargo de Q5 por entrega.

**Requisitos**
- De noche, Ana y Sofía aparecen "Fuera de turno" y no se les puede asignar nada.
  De día, lo mismo con Marta y Diego.
- La tarifa de Marta y Diego incluye el recargo.
- En la lista se distingue quién es del turno de noche.

**Preguntas**
1. ¿Cuántas clases de repartidor tienes ahora? ¿Cuántas tendrías con 3 vehículos y 3
   turnos (por ejemplo, uno de fin de semana)? Dibuja la tabla.
2. ¿En cuántos lugares está escrito el recargo de Q5? Si sube a Q7, ¿cuántos archivos
   cambias?
3. ¿Qué tienen en común la clase de Marta y la de Diego? ¿Por qué no lo pueden compartir?

## Paso 3 · "Mejoras de vehículo"

> Sofía ya hizo varias entregas en bici. Por Q80, cómprenle un carro.

**Requisitos**
- Al presionar **Mejorar** en Sofía se cobran Q80, y Sofía sigue trabajando, pero en
  carro: con su velocidad, su tarifa y su modelo.
- No se pierden el pedido que lleva, el conteo de sus entregas ni su turno.
- Sofía sigue en la lista de repartidores y se le pueden asignar pedidos.
- A quien ya va en carro no se le puede mejorar, y no se le cobra.

*La interfaz llama a un método `public bool Upgrade()` del repartidor, si existe. Debe
devolver `true` si hubo mejora (y entonces se cobran Q80), o `false` si no la hubo.*

**Preguntas**
1. Intenta cumplirlo con el diseño actual. ¿Qué pasa con su pedido? ¿Con sus entregas?
   ¿Con su lugar en la lista?
2. ¿Y si mejoras a Diego? ¿Sigue siendo del turno de noche?
3. ¿Puede un objeto cambiar de clase mientras el programa corre?
4. El vehículo de Sofía, ¿es parte de lo que Sofía **es**, o es algo que Sofía **tiene**?
   ¿Y su turno?

## Paso 4 · Rediseño

> Que se cumplan los pasos 1 a 3, pero sin una clase por cada combinación de vehículo y
> turno.

**Requisitos**
- Todo lo de los pasos 1, 2 y 3 sigue funcionando. Si Sofía se mejora a mitad de una
  entrega, la termina en carro y conserva su pedido y sus entregas.
- Diego, mejorado, sigue en el turno de noche.
- Ninguna clase representa una combinación (como "carro de noche").
- Ningún código pregunta de qué tipo es un vehículo (`is`, o un `if`/`switch` por tipo).
- La interfaz (`GameHUD.cs`) no se modifica.
- El equipo se arma al contratar: nombre, vehículo y turno.

**Preguntas**
1. ¿Qué depende del vehículo? ¿Qué depende del turno? ¿Qué es igual para todos?
2. ¿Cuántas clases necesitas ahora para 2 vehículos y 2 turnos? ¿Y para 3 y 3?
3. Cuando Sofía cambia de vehículo, ¿sigue siendo el mismo objeto?
4. ¿Quién debería saber a qué se mejora una bici?

## Paso 5 · "Ahora también motos"

> Contraten a **Pablo**, que reparte en moto en el turno de noche.

**Requisitos**
- Pablo aparece en la lista, cobra la tarifa de moto con el recargo de noche y solo
  trabaja de noche.
- Se ve en el mapa con el modelo de moto.
- Una moto se puede mejorar a carro.

**Preguntas**
1. ¿Qué archivos creaste? ¿Cuáles modificaste? Compáralo con el paso 1.
2. ¿Cuántas clases nuevas haría falta para agregar un turno de fin de semana?
3. ¿Qué parte del código quedó **cerrada** a cambios y qué parte quedó **abierta** a
   extensiones?

---

## Para cerrar

1. Explica con tus palabras la diferencia entre **heredar** comportamiento y **delegarlo**
   en otro objeto.
2. ¿Cuándo sigue siendo buena idea usar herencia?
3. Un GameObject de Unity **tiene** componentes. ¿Qué principio de hoy usa Unity en todo
   el motor?

## C# para quien viene de Java

| Java | C# |
|---|---|
| `class Dog extends Animal` | `class Dog : Animal` |
| `class Dog implements Pet` | `class Dog : IPet` (las interfaces empiezan con `I`) |
| `abstract` | `abstract` |
| todos los métodos se pueden sobrescribir | solo los marcados `virtual` o `abstract` |
| `@Override` (opcional) | `override` (obligatorio) |
| `getName()` | propiedad `Name { get; }` |
| `return x;` en un método de una línea | `=> x;` |
| `new Dog()` | igual, salvo en componentes de Unity: `AddComponent<Dog>()` |
