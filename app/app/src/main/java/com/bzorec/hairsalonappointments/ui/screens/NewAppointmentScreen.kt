package com.bzorec.hairsalonappointments.ui.screens

import android.content.Intent
import android.provider.CalendarContract
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.KeyboardArrowLeft
import androidx.compose.material.icons.filled.KeyboardArrowRight
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.bzorec.hairsalonappointments.data.api.ApiClient
import com.bzorec.hairsalonappointments.data.model.NewAppointment
import kotlinx.coroutines.launch
import java.time.LocalDate
import java.time.LocalTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import java.util.Locale

@OptIn(ExperimentalMaterial3Api::class, ExperimentalLayoutApi::class)
@Composable
fun NewAppointmentScreen(
    onNavigateBack: () -> Unit = {}
) {
    var customerName by remember { mutableStateOf("") }
    var phone by remember { mutableStateOf("") }
    var selectedService by remember { mutableStateOf("haircut") }
    var selectedStylist by remember { mutableStateOf(1) }
    var selectedDate by remember { mutableStateOf(LocalDate.now()) }
    var selectedTime by remember { mutableStateOf(LocalTime.of(10, 0)) }

    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    val scope = rememberCoroutineScope()
    val snackbarHostState = remember { SnackbarHostState() }
    val context = LocalContext.current

    val services = listOf(
        "haircut" to "Striženje (30 min)",
        "haircut-wash" to "Striženje + pranje (45 min)",
        "color" to "Barvanje (60 min)",
        "highlights" to "Prameni (75 min)",
        "beard" to "Brada (15 min)",
        "blowdry" to "Frizura (20 min)"
    )

    val serviceDurations = mapOf(
        "haircut" to 30,
        "haircut-wash" to 45,
        "color" to 60,
        "highlights" to 75,
        "beard" to 15,
        "blowdry" to 20
    )

    val stylists = listOf(
        1 to "Ana",
        2 to "Tina"
    )

    fun createAppointment() {
        if (customerName.isBlank()) {
            errorMessage = "Prosim vnesi ime stranke"
            return
        }

        scope.launch {
            isLoading = true
            errorMessage = null

            try {
                val startDateTime = selectedDate.atTime(selectedTime)
                    .atZone(ZoneId.systemDefault())
                    .format(DateTimeFormatter.ISO_OFFSET_DATE_TIME)

                val duration = serviceDurations[selectedService] ?: 30
                val endDateTime = selectedDate.atTime(selectedTime)
                    .plusMinutes(duration.toLong())
                    .atZone(ZoneId.systemDefault())
                    .format(DateTimeFormatter.ISO_OFFSET_DATE_TIME)

                val stylistName = stylists.find { it.first == selectedStylist }?.second ?: "Ana"
                val serviceName = services.find { it.first == selectedService }?.second ?: selectedService

                val newAppointment = NewAppointment(
                    title = "$serviceName – $customerName",
                    start = startDateTime,
                    end = endDateTime,
                    resourceId = selectedStylist,
                    phone = phone,
                    service = selectedService,
                    customerName = customerName
                )

                val response = ApiClient.apiService.createAppointment(newAppointment)

                if (response.isSuccessful) {
                    val startMillis = selectedDate.atTime(selectedTime)
                        .atZone(ZoneId.systemDefault()).toInstant().toEpochMilli()
                    val endMillis = selectedDate.atTime(selectedTime)
                        .plusMinutes(duration.toLong())
                        .atZone(ZoneId.systemDefault()).toInstant().toEpochMilli()
                    val calIntent = Intent(Intent.ACTION_INSERT, CalendarContract.Events.CONTENT_URI).apply {
                        putExtra(CalendarContract.Events.TITLE, "$serviceName – $customerName")
                        putExtra(
                            CalendarContract.Events.DESCRIPTION,
                            "Storitev: $serviceName\nStranka: $customerName\nFrizer: $stylistName\nTel: $phone"
                        )
                        putExtra(CalendarContract.EXTRA_EVENT_BEGIN_TIME, startMillis)
                        putExtra(CalendarContract.EXTRA_EVENT_END_TIME, endMillis)
                    }
                    context.startActivity(calIntent)
                    onNavigateBack()
                } else {
                    errorMessage = "Napaka: ${response.code()} - ${response.message()}"
                }
            } catch (e: Exception) {
                errorMessage = "Napaka pri povezavi: ${e.message}"
            } finally {
                isLoading = false
            }
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Nov termin", fontWeight = FontWeight.Bold) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Nazaj")
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer,
                    navigationIconContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        },
        snackbarHost = { SnackbarHost(snackbarHostState) }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(16.dp)
                .verticalScroll(rememberScrollState()),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            OutlinedTextField(
                value = customerName,
                onValueChange = { customerName = it },
                label = { Text("Ime stranke") },
                modifier = Modifier.fillMaxWidth(),
                singleLine = true
            )

            OutlinedTextField(
                value = phone,
                onValueChange = { phone = it },
                label = { Text("Telefon") },
                modifier = Modifier.fillMaxWidth(),
                singleLine = true
            )

            Text("Storitev", style = MaterialTheme.typography.labelLarge)
            services.forEach { (id, name) ->
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(4.dp)
                ) {
                    RadioButton(
                        selected = selectedService == id,
                        onClick = { selectedService = id }
                    )
                    Text(name, style = MaterialTheme.typography.bodyMedium)
                }
            }

            Text("Frizer", style = MaterialTheme.typography.labelLarge)
            stylists.forEach { (id, name) ->
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(4.dp)
                ) {
                    RadioButton(
                        selected = selectedStylist == id,
                        onClick = { selectedStylist = id }
                    )
                    Text(name, style = MaterialTheme.typography.bodyMedium)
                }
            }

            Text("Datum", style = MaterialTheme.typography.labelLarge)
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                IconButton(onClick = { selectedDate = selectedDate.minusDays(1) }) {
                    Icon(Icons.Default.KeyboardArrowLeft, contentDescription = "Prejšnji dan")
                }
                Text(
                    selectedDate.format(DateTimeFormatter.ofPattern("EEEE, d. MMMM", Locale("sl"))),
                    style = MaterialTheme.typography.bodyMedium,
                    fontWeight = FontWeight.Medium
                )
                IconButton(onClick = { selectedDate = selectedDate.plusDays(1) }) {
                    Icon(Icons.Default.KeyboardArrowRight, contentDescription = "Naslednji dan")
                }
            }

            Text("Ura – ${selectedTime.format(DateTimeFormatter.ofPattern("HH:mm"))}", style = MaterialTheme.typography.labelLarge)
            FlowRow(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                verticalArrangement = Arrangement.spacedBy(4.dp)
            ) {
                listOf("09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00").forEach { time ->
                    FilterChip(
                        selected = selectedTime == LocalTime.parse(time),
                        onClick = { selectedTime = LocalTime.parse(time) },
                        label = { Text(time) }
                    )
                }
            }

            if (errorMessage != null) {
                Text(
                    errorMessage!!,
                    color = MaterialTheme.colorScheme.error,
                    style = MaterialTheme.typography.bodyMedium
                )
            }

            Spacer(modifier = Modifier.height(8.dp))

            Button(
                onClick = { createAppointment() },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(52.dp),
                enabled = !isLoading,
                shape = MaterialTheme.shapes.medium
            ) {
                if (isLoading) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(22.dp),
                        color = MaterialTheme.colorScheme.onPrimary,
                        strokeWidth = 2.dp
                    )
                } else {
                    Text("Ustvari termin", fontWeight = FontWeight.SemiBold)
                }
            }
        }
    }
}
