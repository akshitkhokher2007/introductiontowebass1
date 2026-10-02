# Functional Delivery Calculator

**Name:** Akshit Khokher
**Group:** IT-2504

A console app in C# that calculates the final price of a delivery based
on the number of items, delivery type, delivery zone, and whether it's
express or not.

## A note on the assignment file

The "Test cases" table in the assignment document (section 5) uses
columns like Age, Student, Ticket, Day — those don't match this
assignment's inputs (Price, Items, DeliveryType, DeliveryZone, Express).
It looks like it was copied over from a different assignment template
by mistake. Because of that, I wrote my own set of test cases below
that actually match the delivery pricing rules in section 2, instead of
copying numbers that don't apply to this program.

## How to run it

```
cd FunctionalDeliveryCalculator
dotnet run
```

It will then ask for, in this order:
1. Base delivery price (number, e.g. `1500`)
2. Number of items (whole number, e.g. `5`)
3. Express delivery (`true` or `false`)
4. Delivery type (`Pickup`, `Courier`, or `DoorToDoor`)
5. Delivery zone (`City`, `OutsideCity`, or `Remote`)

and print the final price.

## Project structure

- `Program.cs`
  - `Program.Main` — the only place that reads from / writes to the
    console. It validates every input with `TryParse` and returns early
    with an error message if something is invalid.
  - `PriceCalculator` — a static class that does all the math. None of
    its methods touch the console, and there are no global/mutable
    fields — every method just takes values in and returns a value out.
- `FunctionalDeliveryCalculator.csproj` — project file (.NET 8 console app)

## How the price is calculated

Following the order from the assignment (base → items → type → zone → express):

| Step | Rule |
|---|---|
| Items 1–3 | no change |
| Items 4–7 | +10% |
| Items 8+ | +20% |
| Pickup | −20% |
| Courier | no change |
| DoorToDoor | +15% |
| City | no change |
| OutsideCity | +25% |
| Express = true | +30% |

Final price is rounded to 2 decimal places with `Math.Round(price, 2)`.

**Note on `Remote` zone:** the assignment's enum includes a `Remote`
delivery zone, but the pricing rules in section 2 only give explicit
rules for `City` and `OutsideCity`. Since there's no rule written for
`Remote`, this program applies no price change for it (same as `City`).
If the instructor meant for `Remote` to have its own rule, that's a one
line change in `GetDeliveryZoneRule`.

## Test cases

| # | Price | Items | Type | Zone | Express | Expected | Why |
|---|---|---|---|---|---|---|---|
| 1 | 100 | 2 | Courier | City | false | 100.00 | no rule applies at all |
| 2 | 100 | 5 | Courier | City | false | 110.00 | 4–7 items → +10% |
| 3 | 100 | 10 | DoorToDoor | OutsideCity | true | 224.25 | 100 →(+20%)120 →(+15%)138 →(+25%)172.50 →(+30%)224.25 |
| 4 | 200 | 1 | Pickup | City | false | 160.00 | pickup −20% |
| 5 | 50 | 4 | Pickup | OutsideCity | true | 71.50 | 50 →(+10%)55 →(−20%)44 →(+25%)55 →(+30%)71.50 |
| 6 | 0 | 3 | Courier | City | false | 0.00 | zero price stays zero, no crash |
| 7 | abc | 2 | Courier | City | false | error message | text instead of a number for price |
| 8 | 100 | -5 | Courier | City | false | error message | negative item count |
| 9 | 100 | 2 | Bike | City | false | error message | unknown delivery type (not in enum) |
| 10 | -50 | 2 | Courier | City | false | error message | negative price |

Empty input (just pressing Enter) and a `null` string both fail safely
through `TryParse` as well — `decimal.TryParse` and friends simply
return `false` instead of throwing, so the program prints the matching
error message and exits instead of crashing.

## Answers

**1. Which parts of your program handle user input and output?**
Only `Program.Main`. Every `Console.ReadLine`, `Console.Write`, and
`Console.WriteLine` call in the whole project is inside `Main`.

**2. Which functions perform only delivery price calculations?**
Everything inside the `PriceCalculator` class: `CalculateFinalPrice`,
`ApplyRule`, `GetItemCountRule`, `GetDeliveryTypeRule`, and
`GetDeliveryZoneRule`. None of them read from or write to the console,
and none of them depend on any outside/global state — they just take
in values and return a result.

**3. How is `Func<...>` used to apply delivery pricing rules?**
Each pricing step (items, type, zone, express) is represented as a
`Func<decimal, decimal>` — basically "a function that takes a price and
gives back a new price". Instead of writing the math directly inline,
methods like `GetItemCountRule` and `GetDeliveryTypeRule` return the
right `Func<decimal, decimal>` for the situation, and then `ApplyRule`
(the higher-order function) takes a price and one of these `Func`
values and calls it. `CalculateFinalPrice` just chains several
`ApplyRule` calls together, one per rule.

**4. Why is `TryParse` useful when processing delivery data entered by the user?**
Because user input can be anything — empty text, letters instead of
numbers, a typo in an enum name. `TryParse` checks whether the
conversion is possible and returns `true`/`false` instead of throwing
an exception like `Parse` would. That makes it easy to write
`if (!decimal.TryParse(...)) { print error; return; }` and keep the
program from crashing on bad input, while still giving a clear message
about what went wrong.
