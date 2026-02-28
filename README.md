# HairSalonAppointments

Appointment scheduling system for a hair salon with a .NET 10 REST API, an Android management app, and a web booking client.

## Docker

```bash
docker-compose up --build
```

Web client: http://localhost:3000
API: http://localhost:5000/api/v1/

```bash
docker-compose down
```

## Local

**API server** — requires .NET SDK 10+

```bash
cd server
dotnet run --project HairSalonAppointments.Api
```

Starts on http://localhost:5173.

**Web client** — open `client/index.html` in a browser. To use the real API instead of mock mode, expand "Nastavitve" at the bottom, uncheck "Mock način", and set the API base URL to `http://localhost:5173`.

**Android app** — requires Android Studio and JDK 17. Open the `app/` folder in Android Studio and set the base URL in `app/src/main/java/.../data/api/ApiClient.kt`:

```kotlin
// emulator
var baseUrl: String = "http://10.0.2.2:5173/api/v1/"

// physical device (replace with your machine's IP)
// var baseUrl: String = "http://192.168.1.100:5173/api/v1/"
```

When running via Docker use port `5000` instead of `5173`.

## Tests

```bash
cd server
dotnet test
```
