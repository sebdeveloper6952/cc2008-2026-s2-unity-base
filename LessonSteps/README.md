# Live-coding guide — Composition over inheritance + Open/Closed (90 min)

Unit 12 (`slides/12_oop_principles_2`) told as a game: a courier dispatch in the city.
Each step starts with a request from "the client" (the line in quotes is what you
say in class). The folder `stepN_*` holds the finished code for that step.

For students: `lesson-readme.md` has each step's requirements and discussion questions,
without solutions.

## Before class

1. Load the starting point: `./LessonSteps/load_step.sh step0_start`, then open Unity.
2. Open `Assets/Scenes/City.unity`. Set the Game view to **16:9** and **Play Maximized**.
3. Press Play and check that orders appear and couriers deliver them.

**Controls:** click an order (left), then a courier (right). `Esc` cancels, `N` jumps to
dusk or dawn, and **Mejorar · Q80** buys a courier a better vehicle.

**What you edit in class:** only `Assets/Scripts/Couriers/`, plus `GameHUD.cs` once
(step 1). Everything in `Assets/Scripts/Game/` (streets, orders, HUD, clock) is plumbing.

**Safety net:** if the live code breaks, `./LessonSteps/load_step.sh <step>` loads that
step's working code. It keeps the `.meta` files, so the scene doesn't lose references.

## Timeline

| Min | Step | Folder |
|---|---|---|
| 0–5 | Play with cars only | `step0_start` |
| 5–20 | Bikes as a subclass | `step1_bikes` |
| 20–30 | Night shift: subclasses multiply | `step2_night_shift` |
| 30–40 | Vehicle upgrade: no clean way out | `step3_upgrade` |
| 40–65 | Refactor to composition | `step4_composition` |
| 65–75 | Motorcycle: Open/Closed | `step5_motorcycle` |
| 75–90 | Wrap-up, Java ↔ C#, questions | |

### 0 · Cars only
Play. Two cars (Ana, Luis) can't keep up, so orders get lost. Read `CarCourier.cs` with
the class: the first block (`Description`, `Speed`, `Fee`, `ModelPath`) is what depends
on the car; everything below it is what any courier does.

### 1 · "Queremos bicis: más lentas, pero más baratas"
- Extract `abstract class Courier : MonoBehaviour` with everything a courier does. Speed,
  fee, description and model become `abstract`.
- `CarCourier : Courier` and `BikeCourier : Courier` keep only their 4 overrides.
- In `CourierCentral`: `Hire<BikeCourier>("Sofía")`, and change `CarCourier` → `Courier`.
- **It doesn't compile until you also edit `GameHUD.cs`** (find & replace
  `CarCourier` → `Courier`).
- Punchline: *"Para agregar un vehículo tuvimos que abrir la central y la interfaz, código
  que ya funcionaba. Dependían de la clase concreta `CarCourier`."* That's the Open/Closed
  violation. From now on, both depend on `Courier`.
- Show the decision this creates: select an order, and the courier cards show each fee
  (car Q23 vs bike Q12 for 2 km).

### 2 · "Los de siempre trabajan de día. Contratamos turno noche, con Q5 de recargo"
- In `Courier`: `public virtual bool IsOnDuty(bool isNight) => !isNight;`
- `NightCarCourier : CarCourier` and `NightBikeCourier : BikeCourier`, each overriding the
  **same three things** (description, `IsOnDuty`, `Fee` + 5).
- Hire Marta (night car) and Diego (night bike). Press `N`: day couriers show "Fuera de
  turno", and night fees are +Q5.
- Punchline: 2 vehicles × 2 shifts = 4 classes, with the `+ 5` copied twice. *"¿Y si
  llega la moto? ¿Y un turno de fin de semana?"* Draw the 3 × 3 grid on the board.

### 3 · "Sofía ya hizo varias entregas en bici. Por Q80, cómprenle un carro"
- Press **Mejorar** on Sofía first: *"Todavía no se pueden comprar mejoras."*
- The HUD calls `public bool Upgrade()` if the courier has it, and charges Q80 only when it
  returns `true`.
- In `Courier`: `public virtual bool Upgrade() => false;` (by default, no upgrade), and
  change `parkingSpot` to `protected`.
- In `BikeCourier`, override `Upgrade()` with the "fix" (see `step3_upgrade/BikeCourier.cs`):
  destroy the component, add a `CarCourier`, return `true`.
- Mejorar on Ana: *"Ana ya tiene el mejor vehículo"*, and she isn't charged.
- Assign Sofía an order, then press Mejorar. What students see:
  - Q80 is charged, and Sofía **teleports** to the parking lot in a car.
  - Sofía **disappears from the courier list**, because the central still points to the
    destroyed object.
  - Her order is stuck until it's lost, and her delivery count resets.
- Bonus question: *"¿Y si mejoramos a Diego?"* `NightBikeCourier` inherits the override,
  so he becomes a **day** `CarCourier`. Keeping his shift would need yet another override
  that turns him into a `NightCarCourier`: the class explosion again.
- Punchline: *"Para darle un carro a Sofía, ¿la despedimos y contratamos a otra Sofía?
  Un objeto no puede cambiar de clase."* The vehicle was part of **what Sofía is**, when
  it should be something she **has**.

### 4 · Refactor: Ana TIENE un vehículo y TIENE un turno
- `IVehicle` → `Car`, `Bike`. `IShift` → `DayShift`, `NightShift`. These are plain C#
  classes created with `new`, one file each, like in Java.
- `Courier` is no longer abstract. It holds `IVehicle vehicle; IShift shift;` and
  **delegates**: `Fee = vehicle.Fee(km) + shift.Surcharge`, `IsOnDuty = shift.IsOnDuty(...)`.
- `ChangeVehicle(IVehicle)` swaps the vehicle, the model and the speed.
- Each vehicle says what it upgrades to: `IVehicle Upgrade()` in the interface, `Bike`
  returns `new Car()`, `Car` returns `null`. `Courier.Upgrade()` asks its vehicle and calls
  `ChangeVehicle`. No `if (vehicle is Bike)`: the bike decides (unit 10, replacing
  conditionals with polymorphism).
- `CourierCentral.Hire("Ana", new Car(), new DayShift())`, the same as the slide's
  `new Courier("Ana", new Motorcycle(), new NightShift())`.
- Delete the 4 old subclasses. **`GameHUD.cs` is not touched.**
- Mejorar on Sofía mid-delivery: she becomes a car on the spot, **keeps her order and her
  deliveries**, and finishes the trip faster. Mejorar on Diego: "Carro · Noche". He keeps
  his shift and its +Q5, with no extra class.
- Punchline: 2 + 2 classes instead of 2 × 2, and the combination is chosen when hiring
  and can change while the game runs.

### 5 · "Ahora también motos"
- One new class: `Motorcycle : IVehicle` (Q10 + Q3/km, as on the slide), which upgrades
  to a car (`Upgrade() => new Car()`).
- `Hire("Pablo", new Motorcycle(), new NightShift())`.
- Neither `Courier`, `Assign` nor the HUD changed: **open to extension, closed to
  modification.** Good one for students to type in pairs.

### Wrap-up
- *"Programe contra una interfaz, arme el comportamiento con objetos, y agregue casos con
  clases nuevas, no con `if` nuevos."*
- Unity itself is built on composition: a GameObject **has** components.

## Java ↔ C# for this lesson

| Java | C# |
|---|---|
| `class Car extends Vehicle` | `class Car : Vehicle` |
| `class Car implements IVehicle` | `class Car : IVehicle` |
| methods are virtual by default | `virtual` in the parent, `override` in the child |
| `@Override` | `override` (a keyword, and required) |
| `getSpeed()` | `Speed { get; }` property; `=> 20f` is a one-line body |
| `List<Courier>` | `List<Courier>` (the same) |
| constructor `new Courier(...)` | components can't use constructors → `AddComponent<Courier>()` + `Setup(...)` |
